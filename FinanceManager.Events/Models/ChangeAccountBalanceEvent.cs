using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.Events.Models;

public class ChangeAccountBalanceEvent : IRejectableEvent
{
    public string TransactionId { get; set; } = null!;
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public string? UserAddress { get; set; }

    public IEvent GetRejectEvent()
    {
        return new ChangeAccountBalanceRejectEvent
        {
            TransactionId = TransactionId
        };
    }
}