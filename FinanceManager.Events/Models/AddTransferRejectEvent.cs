using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.Events.Models;

public class AddTransferRejectEvent : IEvent
{
    public string TransactionId { get; set; }
}