using FinanceManager.Events.Models;
using FinanceManager.TransportLibrary.Services;

namespace FinanceManager.Account.Domain;

public class Account
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = null!;
    public long UserId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Balance { get; set; }
    public Guid CurrencyId { get; set; }
    public string? Icon { get; set; }
    public string? Description { get; set; }

    public bool ValidateAndUpdateBalance(
        AccountLimit[] limits,
        decimal changeValue,
        DateTime changeDate,
        Guid changeCategoryId,
        Guid accountId,
        string? userAddress,
        string transactionId,
        ILogger logger,
        IMessagePublisher<NotificationSendEvent> notificationSendPublisher,
        IMessagePublisher<ChangeStatisticsEvent> changeStatisticsPublisher)
    {
        var newBalanceValue = Balance + changeValue;
        if (newBalanceValue < 0)
        {
            logger.LogWarning(
                "Account with id {AccountId} balance less zero",
                accountId);
            if (userAddress != null)
            {
                notificationSendPublisher.Send(new NotificationSendEvent
                {
                    TransactionId = transactionId,
                    Type = NotificationType.Warning,
                    Title = "Account balance less zero",
                    Body = "Account balance less zero",
                    ToAddress = userAddress
                });
            }

            return false;
        }

        var workLimit = limits.Where(x => x.LimitValue <= newBalanceValue).ToArray();
        if (workLimit.Any())
        {
            var notifyLimit = workLimit
                .Where(x => x.IsNotification)
                .MinBy(x => x.LimitValue);
            if (notifyLimit != null && userAddress != null)
            {
                notificationSendPublisher.Send(new NotificationSendEvent
                {
                    TransactionId = transactionId,
                    Type = NotificationType.Warning,
                    Title = "Account can't be changed",
                    Body = $"Limit value {notifyLimit.LimitValue} exceeded",
                    ToAddress = userAddress
                });
            }

            var restrictedLimit = workLimit
                .Where(x => x.IsRestriction)
                .MinBy(x => x.LimitValue);
            if (restrictedLimit != null)
            {
                logger.LogWarning(
                    "Changing account with id {AccountId} balance has been restricted by limit value {LimitValue}",
                    accountId,
                    restrictedLimit.LimitValue);
                return false;
            }
        }

        Balance = newBalanceValue;
        changeStatisticsPublisher.Send(new ChangeStatisticsEvent
        {
            TransactionId = transactionId,
            AccountId = accountId,
            CategoryId = changeCategoryId,
            Date = changeDate,
            Value = changeValue
        });

        return true;
    }
}