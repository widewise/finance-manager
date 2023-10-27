using FinanceManager.Events;
using FinanceManager.Events.Models;
using FinanceManager.Expense.Models;
using FinanceManager.TransportLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Expense.Services;

public interface IExpenseService
{
    Task<Models.Expense[]> GetAsync(ExpenseQueryParameters parameters);
    Task<Models.Expense?> CreateAsync(string requestId,
        long userId,
        string? userAddress,
        CreateUpdateExpenseModel model);
    Task<bool> UpdateAsync(
        Guid id,
        string requestId,
        string userAddress,
        CreateUpdateExpenseModel model);
    Task<bool> DeleteAsync(Guid id);
    Task RejectAsync(string requestId);
}

public class ExpenseService : IExpenseService
{
    private readonly ILogger<ExpenseService> _logger;
    private readonly AppDbContext _appDbContext;
    private readonly IMessagePublisher<ChangeAccountBalanceEvent> _changeAccountBalancePublisher;
    private readonly IMessagePublisher<AddExpenseRejectEvent> _addExpenseRejectPublisher;

    public ExpenseService(
        ILogger<ExpenseService> logger,
        AppDbContext appDbContext,
        IMessagePublisher<ChangeAccountBalanceEvent> changeAccountBalancePublisher,
        IMessagePublisher<AddExpenseRejectEvent> addExpenseRejectPublisher)
    {
        _logger = logger;
        _appDbContext = appDbContext;
        _changeAccountBalancePublisher = changeAccountBalancePublisher;
        _addExpenseRejectPublisher = addExpenseRejectPublisher;
    }

    public async Task<Models.Expense[]> GetAsync(ExpenseQueryParameters parameters)
    {
        var query = _appDbContext.Expenses.Where(x => true);
        if (parameters.RequestId != null)
        {
            query = query.Where(x => x.RequestId == parameters.RequestId);
        }

        if (parameters.Id.HasValue)
        {
            query = query.Where(x => x.Id == parameters.Id.Value);
        }

        if (parameters.AccountId.HasValue)
        {
            query = query.Where(x => x.AccountId == parameters.AccountId.Value);
        }

        if (parameters.CategoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == parameters.CategoryId.Value);
        }

        if (parameters.Date.HasValue)
        {
            query = query.Where(x => x.Date == parameters.Date.Value.Date);
        }

        if (parameters.UserId != null)
        {
            query = query.Where(x => x.UserId == parameters.UserId);
        }

        return await query.ToArrayAsync();
    }

    public async Task<Models.Expense?> CreateAsync(
        string requestId,
        long userId,
        string? userAddress,
        CreateUpdateExpenseModel model)
    {
        try
        {
            if (await _appDbContext.Expenses.AnyAsync(x => x.RequestId == requestId && x.Id == model.Id))
            {
                _logger.LogWarning(
                    "Expense has already created for request with id {RequestId}",
                    requestId);
                return null;
            }

            var newExpense = new Models.Expense
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                UserId = userId,
                AccountId = model.AccountId,
                CategoryId = model.CategoryId,
                Date = model.Date,
                Value = model.Value
            };
            if (model.Id.HasValue)
            {
                newExpense.Id = model.Id.Value;
            }
            var created = await _appDbContext.AddAsync(newExpense);

            await _appDbContext.SaveChangesAsync();
            if (model.CategoryId != TransferConstants.TransferCategoryId)
            {
                _changeAccountBalancePublisher.Send(new ChangeAccountBalanceEvent
                {
                    TransactionId = requestId,
                    AccountId = model.AccountId,
                    CategoryId = model.CategoryId,
                    Date = model.Date,
                    Value = -model.Value,
                    UserAddress = userAddress
                });
            }

            return created.Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Create expense error: {ErrorMessage}",
                e.Message);
            return null;
        }
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        string requestId,
        string userAddress,
        CreateUpdateExpenseModel model)
    {
        var existed = await _appDbContext.Expenses.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("Expense with id {d} is not found", id);
            return false;
        }

        if (existed.CategoryId == TransferConstants.TransferCategoryId && existed.CategoryId != model.CategoryId)
        {
            _logger.LogWarning("Changing transfer expense category is forbidden");
            return false;
        }

        var valueDiff = model.Value - existed.Value;
        existed.AccountId = model.AccountId;
        existed.CategoryId = model.CategoryId;
        existed.Date = model.Date;
        existed.Value = model.Value;
        _appDbContext.Expenses.Update(existed);
        await _appDbContext.SaveChangesAsync();
        _changeAccountBalancePublisher.Send(new ChangeAccountBalanceEvent
        {
            TransactionId = requestId,
            AccountId = model.AccountId,
            CategoryId = model.CategoryId,
            Date = model.Date,
            Value = -valueDiff,
            UserAddress = userAddress
        });
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existed = await _appDbContext.Expenses.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("Expense with id {d} is not found", id);
            return false;
        }

        _appDbContext.Expenses.Remove(existed);
        await _appDbContext.SaveChangesAsync();
        return true;
    }

    public async Task RejectAsync(string requestId)
    {
        try
        {
            var expenses = await _appDbContext.Expenses
                .Where(x => x.RequestId == requestId)
                .ToArrayAsync();
            if (!expenses.Any())
            {
                return;
            }

            _appDbContext.Expenses.RemoveRange(expenses);
            await _appDbContext.SaveChangesAsync();
            _addExpenseRejectPublisher.Send(new AddExpenseRejectEvent
            {
                TransactionId = requestId
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Reject expense with request id {RequestId} error: {ErrorMessage}",
                requestId,
                e.Message);
        }
    }
}