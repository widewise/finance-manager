using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.Events.Models;

public class ChangeAccountBalanceRejectEvent : IEvent
{
    public string TransactionId { get; set; } = null!;
}