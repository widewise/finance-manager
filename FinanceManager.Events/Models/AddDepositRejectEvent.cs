using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.Events.Models;

public class AddDepositRejectEvent : IEvent
{
    public string TransactionId { get; set; } = null!;
}