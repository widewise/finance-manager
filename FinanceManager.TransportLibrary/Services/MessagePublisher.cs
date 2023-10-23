using System.Text.Json;
using RabbitMQ.Client;

namespace FinanceManager.TransportLibrary.Services;

public class MessagePublisher<TMessage> : IMessagePublisher<TMessage>
{
    private readonly IModel _channel;
    private readonly string _exchangeName;
    private readonly string _routingKey;
    private readonly IBasicProperties _props;

    public MessagePublisher(
        IModel channel,
        string exchangeName,
        string routingKey)
    {
        _channel = channel;
        _exchangeName = exchangeName;
        _routingKey = routingKey;
        _props = _channel.CreateBasicProperties();
        _props.ContentType = "text/plain";
        _props.DeliveryMode = 2;
    }

    public void Send<TMessage>(TMessage message)
    {
        var serializedMessageString = JsonSerializer.Serialize(message);
        var messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(serializedMessageString);
        _channel.BasicPublish(_exchangeName, _routingKey, _props, messageBodyBytes);
    }
}