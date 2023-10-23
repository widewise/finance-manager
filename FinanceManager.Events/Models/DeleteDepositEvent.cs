using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.Events.Models;

public class DeleteDepositEvent : IEvent
{
    public string TransactionId { get; set; } = null!;
    public Guid Id { get; set; }
}