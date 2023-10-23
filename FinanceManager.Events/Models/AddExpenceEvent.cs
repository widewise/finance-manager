using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.Events.Models;

public class AddExpenseEvent : IRejectableEvent
{
    public IEvent GetRejectEvent()
    {
        return new AddExpenseRejectEvent
        {
            TransactionId = TransactionId
        };
    }
    public Guid? Id { get; set; }
    public string TransactionId { get; set; } = null!;
    public Guid AccountId { get; set; }
    public long UserId { get; set; }
    public string? UserAddress { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}