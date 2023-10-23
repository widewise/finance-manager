using FinanceManager.Deposit.Models;
using FinanceManager.Events;
using FinanceManager.Events.Models;
using FinanceManager.TransportLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Deposit.Services;

public interface IDepositService
{
    Task<Models.Deposit[]> GetAsync(DepositQueryParameters parameters);
    Task<Models.Deposit?> CreateAsync(
        string requestId,
        long userId,
        string? userAddress,
        CreateUpdateDepositModel model);
    Task<bool> UpdateAsync(
        Guid id,
        string requestId,
        string userAddress,
        CreateUpdateDepositModel model);
    Task<bool> DeleteAsync(Guid id);
    Task RejectAsync(string requestId);
}

public class DepositService : IDepositService
{
    private readonly ILogger<DepositService> _logger;
    private readonly AppDbContext _appDbContext;
    private readonly IMessagePublisher<ChangeAccountBalanceEvent> _changeAccountBalancePublisher;
    private readonly IMessagePublisher<AddDepositRejectEvent> _addDepositRejectPublisher;

    public DepositService(
        ILogger<DepositService> logger,
        AppDbContext appDbContext,
        IMessagePublisher<ChangeAccountBalanceEvent> changeAccountBalancePublisher,
        IMessagePublisher<AddDepositRejectEvent> addDepositRejectPublisher)
    {
        _logger = logger;
        _appDbContext = appDbContext;
        _changeAccountBalancePublisher = changeAccountBalancePublisher;
        _addDepositRejectPublisher = addDepositRejectPublisher;
    }

    public async Task<Models.Deposit[]> GetAsync(DepositQueryParameters parameters)
    {
        var query = _appDbContext.Deposits.Where(x => true);
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

        if (parameters.RequestId != null)
        {
            query = query.Where(x => x.RequestId == parameters.RequestId);
        }

        if (parameters.UserId != null)
        {
            query = query.Where(x => x.UserId == parameters.UserId);
        }

        return await query.ToArrayAsync();
    }

    public async Task<Models.Deposit?> CreateAsync(
        string requestId,
        long userId,
        string? userAddress,
        CreateUpdateDepositModel model)
    {
        try
        {
            if (await _appDbContext.Deposits.AnyAsync(x => x.RequestId == requestId && x.Id == model.Id))
            {
                _logger.LogWarning(
                    "Deposit has already created for request with id {RequestId}",
                    requestId);
                return null;
            }

            var newObject = new Models.Deposit
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
                newObject.Id = model.Id.Value;
            }
            var created = await _appDbContext.AddAsync(newObject);

            await _appDbContext.SaveChangesAsync();
            if (model.CategoryId != TransferConstants.TransferCategoryId)
            {
                _changeAccountBalancePublisher.Send(new ChangeAccountBalanceEvent
                {
                    TransactionId = requestId,
                    AccountId = model.AccountId,
                    CategoryId = model.CategoryId,
                    Date = model.Date,
                    Value = model.Value,
                    UserAddress = userAddress
                });
            }

            return created.Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Create deposit error: {ErrorMessage}",
                e.Message);
            return null;
        }
    }

    public async Task<bool> UpdateAsync(
        Guid id,
        string requestId,
        string userAddress,
        CreateUpdateDepositModel model)
    {
        var existed = await _appDbContext.Deposits.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("Deposit with id {d} is not found", id);
            return false;
        }

        if (existed.CategoryId == TransferConstants.TransferCategoryId && existed.CategoryId != model.CategoryId)
        {
            _logger.LogWarning("Changing transfer deposit category is forbidden");
            return false;
        }

        var valueDiff = model.Value - existed.Value;
        existed.AccountId = model.AccountId;
        existed.CategoryId = model.CategoryId;
        existed.Date = model.Date;
        existed.Value = model.Value;
        _appDbContext.Deposits.Update(existed);
        await _appDbContext.SaveChangesAsync();
        _changeAccountBalancePublisher.Send(new ChangeAccountBalanceEvent
        {
            TransactionId = requestId,
            AccountId = model.AccountId,
            CategoryId = model.CategoryId,
            Date = model.Date,
            Value = valueDiff,
            UserAddress = userAddress
        });

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existed = await _appDbContext.Deposits.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("Deposit with id {d} is not found", id);
            return false;
        }

        _appDbContext.Deposits.Remove(existed);
        await _appDbContext.SaveChangesAsync();
        return true;
    }

    public async Task RejectAsync(string requestId)
    {
        try
        {
            var deposits = await _appDbContext.Deposits
                .Where(x => x.RequestId == requestId)
                .ToArrayAsync();
            if (!deposits.Any())
            {
                return;
            }

            _appDbContext.Deposits.RemoveRange(deposits);
            await _appDbContext.SaveChangesAsync();
            _addDepositRejectPublisher.Send(new AddDepositRejectEvent
            {
                TransactionId = requestId
            });
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Reject deposit with request id {RequestId} error: {ErrorMessage}",
                requestId,
                e.Message);
        }
    }
}