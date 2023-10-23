namespace FinanceManager.TransportLibrary.Models;

/// <summary>
/// Rejectable event model
/// </summary>
public interface IRejectableEvent : IEvent
{
    /// <summary>
    /// Get rejectable <see cref="IEvent"/> instance
    /// </summary>
    /// <returns>The <see cref="IEvent"/> instance</returns>
    IEvent GetRejectEvent();
}