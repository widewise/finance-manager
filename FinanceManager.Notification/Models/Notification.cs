using System.ComponentModel.DataAnnotations;
using FinanceManager.Events.Models;

namespace FinanceManager.Notification.Models;

public class Notification
{
    [Key]
    public long Id { get; set; }
    public string TransactionId { get; set; } = null!;
    public NotificationType Type { get; set; }
    public string ToAddress { get; set; } = null!;
    public string FromAddress { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
}