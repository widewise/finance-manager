using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.Events.Models;

public class AddTransferEvent : IRejectableEvent
{
    public IEvent GetRejectEvent()
    {
        return new AddTransferRejectEvent
        {
            TransactionId = TransactionId
        };
    }

    public Guid Id { get; set; }
    public string TransactionId { get; set; } = null!;
    public long UserId { get; set; }
    public string? UserAddress { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public DateTime Date { get; set; }
    public decimal FromValue { get; set; }
    public decimal ToValue { get; set; }
    public string? Description { get; set; }
}