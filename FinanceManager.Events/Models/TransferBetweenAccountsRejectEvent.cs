using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.Events.Models;

public class TransferBetweenAccountsRejectEvent : IEvent
{
    public string TransactionId { get; set; } = null!;
}