namespace FinanceManager.TransportLibrary.Models;

internal class TransportInfo
{
    public TransportInfo(
        string exchangeName,
        string queueName,
        string eventName,
        Type consumer)
    {
        ExchangeName = exchangeName;
        QueueName = queueName;
        EventName = eventName;
        Consumer = consumer;
    }

    public string ExchangeName { get; }
    public string QueueName { get; }
    public string EventName { get; }
    public Type Consumer { get; }
}