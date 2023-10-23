using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.Events.Models;

public class ChangeStatisticsEvent : IEvent
{
    public string TransactionId { get; set; } = null!;
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}