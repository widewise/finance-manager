namespace FinanceManager.TransportLibrary.Services;

/// <summary>
/// Message <typeparam name="TMessage">Message type</typeparam> publisher
/// </summary>
/// <typeparam name="TMessage">Message type</typeparam>
public interface IMessagePublisher<TMessage>
{
    /// <summary>
    /// Send <typeparam name="TMessage">Message type</typeparam> instance
    /// </summary>
    /// <param name="message">The <typeparam name="TMessage">Message type</typeparam> instance</param>
    /// <typeparam name="TMessage">Message type</typeparam>
    void Send<TMessage>(TMessage message);
}