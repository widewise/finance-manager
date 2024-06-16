using FinanceManager.Events;
using FinanceManager.Events.Models;
using FinanceManager.Transfer.Models;
using FinanceManager.TransportLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Transfer.Services;

public interface ITransferService
{
    Task<Models.Transfer[]> GetAsync(TransferQueryParameters parameters);
    Task<Models.Transfer?> CreateAsync(
        string requestId,
        long userId,
        string? userAddress,
        CreateTransferModel model);
    Task<bool> UpdateAsync(Guid id, UpdateTransferModel model);
    Task<bool> DeleteAsync(Guid id);
    Task RejectAsync(string requestId);
}

public class TransferService : ITransferService
{
    private readonly ILogger<TransferService> _logger;
    private readonly AppDbContext _appDbContext;
    private readonly IMessagePublisher<AddDepositEvent> _addDepositPublisher;
    private readonly IMessagePublisher<AddExpenseEvent> _addExpensePublisher;
    private readonly IMessagePublisher<DeleteDepositEvent> _deleteDepositPublisher;
    private readonly IMessagePublisher<DeleteExpenseEvent> _deleteExpensePublisher;
    private readonly IMessagePublisher<TransferBetweenAccountsEvent> _transferBetweenAccountsPublisher;
    private readonly IMessagePublisher<AddTransferRejectEvent> _addTransferRejectEvent;

    public TransferService(
        ILogger<TransferService> logger,
        AppDbContext appDbContext,
        IMessagePublisher<AddDepositEvent> addDepositPublisher,
        IMessagePublisher<AddExpenseEvent> addExpensePublisher,
        IMessagePublisher<DeleteDepositEvent> deleteDepositPublisher,
        IMessagePublisher<DeleteExpenseEvent> deleteExpensePublisher,
        IMessagePublisher<TransferBetweenAccountsEvent> transferBetweenAccountsPublisher,
        IMessagePublisher<AddTransferRejectEvent> addTransferRejectEvent)
    {
        _logger = logger;
        _appDbContext = appDbContext;
        _addDepositPublisher = addDepositPublisher;
        _addExpensePublisher = addExpensePublisher;
        _deleteDepositPublisher = deleteDepositPublisher;
        _deleteExpensePublisher = deleteExpensePublisher;
        _transferBetweenAccountsPublisher = transferBetweenAccountsPublisher;
        _addTransferRejectEvent = addTransferRejectEvent;
    }

    public async Task<Models.Transfer[]> GetAsync(TransferQueryParameters parameters)
    {
        var query = _appDbContext.Transfers.Where(x => true);
        if (parameters.Id.HasValue)
        {
            query = query.Where(x => x.Id == parameters.Id.Value);
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

    public async Task<Models.Transfer?> CreateAsync(
        string requestId,
        long userId,
        string? userAddress,
        CreateTransferModel model)
    {
        try
        {
            if (await _appDbContext.Transfers.AnyAsync(x => x.RequestId == requestId && x.Id == model.Id))
            {
                _logger.LogWarning(
                    "Transfer has already created for request with id {RequestId}",
                    requestId);
                return null;
            }

            var addDepositEvent = new AddDepositEvent
            {
                Id = Guid.NewGuid(),
                TransactionId = requestId,
                UserId = userId,
                UserAddress = userAddress,
                AccountId = model.ToAccountId,
                CategoryId = TransferConstants.TransferCategoryId,
                Date = model.Date,
                Value = model.ToValue
            };
            var addExpenseEvent = new AddExpenseEvent
            {
                Id = Guid.NewGuid(),
                TransactionId = requestId,
                UserId = userId,
                UserAddress = userAddress,
                AccountId = model.FromAccountId,
                CategoryId = TransferConstants.TransferCategoryId,
                Date = model.Date,
                Value = model.FromValue
            };
            var transferBetweenAccountsEvent = new TransferBetweenAccountsEvent
            {
                TransactionId = requestId,
                FromAccountId = model.FromAccountId,
                ToAccountId = model.ToAccountId,
                FromValue = model.FromValue,
                ToValue = model.ToValue,
                UserAddress = userAddress,
                Date = model.Date
            };
            var created = await _appDbContext.AddAsync(new Models.Transfer
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                UserId = userId,
                DepositId = addDepositEvent.Id.Value,
                ExpenseId = addExpenseEvent.Id.Value,
                Description = model.Description
            });

            await _appDbContext.SaveChangesAsync();
            await _addDepositPublisher.SendAsync(addDepositEvent);
            await _addExpensePublisher.SendAsync(addExpenseEvent);
            await _transferBetweenAccountsPublisher.SendAsync(transferBetweenAccountsEvent);

            return created.Entity;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Create transfer error: {ErrorMessage}",
                e.Message);
            return null;
        }
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateTransferModel model)
    {
        var existed = await _appDbContext.Transfers.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("Transfer with id {Id} is not found", id);
            return false;
        }

        existed.Description = model.Description;
        _appDbContext.Transfers.Update(existed);
        await _appDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existed = await _appDbContext.Transfers.FirstOrDefaultAsync(x => x.Id == id);
        if (existed == null)
        {
            _logger.LogWarning("Transfer with id {Id} is not found", id);
            return false;
        }

        _appDbContext.Transfers.Remove(existed);
        await _appDbContext.SaveChangesAsync();
        await _deleteDepositPublisher.SendAsync(new DeleteDepositEvent
        {
            TransactionId = existed.RequestId,
            Id = existed.DepositId
        });
        await _deleteExpensePublisher.SendAsync(new DeleteExpenseEvent
        {
            TransactionId = existed.RequestId,
            Id = existed.ExpenseId
        });
        return true;
    }
    
    public async Task RejectAsync(string requestId)
    {
        try
        {
            var transfers = await _appDbContext.Transfers
                .Where(x => x.RequestId == requestId)
                .ToArrayAsync();
            if (!transfers.Any())
            {
                return;
            }

            _appDbContext.Transfers.RemoveRange(transfers);
            await _appDbContext.SaveChangesAsync();
            await _addTransferRejectEvent.SendAsync(new AddTransferRejectEvent
            {
                TransactionId = requestId
            });
            foreach (var transfer in transfers)
            {
                await _deleteDepositPublisher.SendAsync(new DeleteDepositEvent
                {
                    TransactionId = requestId,
                    Id = transfer.DepositId
                });
                await _deleteExpensePublisher.SendAsync(new DeleteExpenseEvent
                {
                    TransactionId = requestId,
                    Id = transfer.ExpenseId
                });
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Reject transfer with request id {RequestId} error: {ErrorMessage}",
                requestId,
                e.Message);
        }
    }
}