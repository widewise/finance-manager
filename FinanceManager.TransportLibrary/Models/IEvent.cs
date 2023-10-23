namespace FinanceManager.TransportLibrary.Models;

/// <summary>
/// Event model
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Transaction identifier
    /// </summary>
    public string TransactionId { get; set; }
}