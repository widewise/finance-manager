using FinanceManager.TransportLibrary.Models;

namespace FinanceManager.Events.Models;

public class AddExpenseRejectEvent : IEvent
{
    public string TransactionId { get; set; } = null!;
}