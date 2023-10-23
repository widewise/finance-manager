using FinanceManager.Account.Models;
using FinanceManager.Events;
using FinanceManager.Events.Models;
using FinanceManager.TransportLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.Account.Services;

public interface IAccountBalanceService
{
    Task<bool> UpdateAccountBalanceAsync(UpdateAccountBalanceModel model);
    Task<bool> TransferBetweenAccountsAsync(TransferBetweenAccountsModel model);
}

public class AccountBalanceService : IAccountBalanceService
{
    private readonly ILogger<AccountService> _logger;
    private readonly AppDbContext _appDbContext;
    private readonly IMessagePublisher<NotificationSendEvent> _notificationSendPublisher;
    private readonly IMessagePublisher<ChangeStatisticsEvent> _changeStatisticsPublisher;

    public AccountBalanceService(
        ILogger<AccountService> logger,
        AppDbContext appDbContext,
        IMessagePublisher<NotificationSendEvent> notificationSendPublisher,
        IMessagePublisher<ChangeStatisticsEvent> changeStatisticsPublisher)
    {
        _logger = logger;
        _appDbContext = appDbContext;
        _notificationSendPublisher = notificationSendPublisher;
        _changeStatisticsPublisher = changeStatisticsPublisher;
    }

    public async Task<bool> UpdateAccountBalanceAsync(UpdateAccountBalanceModel model)
    {
        try
        {
            if (!await InternalUpdateAccountBalanceAsync(model))
            {
                return false;
            }

            await _appDbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Updating account with id {AccountId} balance error: {ErrorMessage}",
                model.AccountId,
                e.Message);
            return false;
        }
    }

    public async Task<bool> TransferBetweenAccountsAsync(TransferBetweenAccountsModel model)
    {
        try
        {
            if (!await InternalUpdateAccountBalanceAsync(new UpdateAccountBalanceModel
                {
                    TransactionId = model.TransactionId,
                    AccountId = model.FromAccountId,
                    CategoryId = TransferConstants.TransferCategoryId,
                    Date = model.Date,
                    Value = -model.FromValue,
                    UserAddress = model.UserAddress,
                }))
            {
                return false;
            }

            if (!await InternalUpdateAccountBalanceAsync(new UpdateAccountBalanceModel
                {
                    TransactionId = model.TransactionId,
                    AccountId = model.ToAccountId,
                    CategoryId = TransferConstants.TransferCategoryId,
                    Date = model.Date,
                    Value = model.ToValue,
                    UserAddress = model.UserAddress,
                }))
            {
                return false;
            }

            await _appDbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Transfer from account with id {FromAccountId} to account with id {ToAccountId} error: {ErrorMessage}",
                model.FromAccountId,
                model.ToAccountId,
                e.Message);
            return false;
        }
    }

    private async Task<bool> InternalUpdateAccountBalanceAsync(UpdateAccountBalanceModel model)
    {
        var account = await _appDbContext.Accounts.FirstOrDefaultAsync(x => x.Id == model.AccountId);
        if (account == null)
        {
            _logger.LogWarning("Account with id {AccountId} is not found", model.AccountId);
            return false;
        }

        var limits = _appDbContext.AccountLimits
            .Where(x => x.AccountId == model.AccountId);
        var newBalanceValue = account.Balance + model.Value;
        if (newBalanceValue < 0)
        {
            _logger.LogWarning(
                "Account with id {AccountId} balance less zero",
                model.AccountId);
            if (model.UserAddress != null)
            {
                _notificationSendPublisher.Send(new NotificationSendEvent
                {
                    TransactionId = model.TransactionId,
                    Type = NotificationType.Warning,
                    Title = "Account balance less zero",
                    Body = "Account balance less zero",
                    ToAddress = model.UserAddress
                });
            }

            return false;
        }
        var workLimit = limits.Where(x => x.LimitValue <= newBalanceValue).ToArray();
        if (workLimit.Any())
        {
            var notifyLimit = workLimit
                .Where(x => x.Type == AccountLimitType.Notify)
                .MinBy(x => x.LimitValue);
            if (notifyLimit != null && model.UserAddress != null)
            {
                _notificationSendPublisher.Send(new NotificationSendEvent
                {
                    TransactionId = model.TransactionId,
                    Type = NotificationType.Warning,
                    Title = "Account can't be changed",
                    Body = $"Limit value {notifyLimit.LimitValue} exceeded",
                    ToAddress = model.UserAddress
                });
            }

            var restrictedLimit = workLimit
                .Where(x => x.Type == AccountLimitType.Restrict)
                .MinBy(x => x.LimitValue);
            if (restrictedLimit != null)
            {
                _logger.LogWarning(
                    "Changing account with id {AccountId} balance has been restricted by limit value {LimitValue}",
                    model.AccountId,
                    restrictedLimit.LimitValue);
                return false;
            }
        }

        account.Balance = newBalanceValue;
        _appDbContext.Update(account);
        _changeStatisticsPublisher.Send(new ChangeStatisticsEvent
        {
            TransactionId = model.TransactionId,
            AccountId = model.AccountId,
            CategoryId = model.CategoryId,
            Date = model.Date,
            Value = model.Value
        });

        return true;
    }
}