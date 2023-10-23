using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.Events.Models;

public class NotificationSendEvent: IEvent
{
    public string TransactionId { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string ToAddress { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
}

public enum NotificationType
{
    Successed = 0,
    Failed = 1,
    Warning = 2
}