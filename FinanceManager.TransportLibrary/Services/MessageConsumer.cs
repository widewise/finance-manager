using System.Text;
using System.Text.Json;
using FinanceManager.TransportLibrary.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinanceManager.TransportLibrary.Services;

public abstract class MessageConsumer<TMessage, TRevertMessage>: AsyncEventingBasicConsumer
    where TMessage : IEvent
    where TRevertMessage : class, IEvent

{
    private const int RetryMaxCount = 3;
    private readonly ILogger<MessageConsumer<TMessage, TRevertMessage>> _logger;
    private readonly IModel _channel;
    private readonly IMessagePublisher<TRevertMessage>? _revertMessagePublisher;

    protected MessageConsumer(
        ILogger<MessageConsumer<TMessage, TRevertMessage>> logger,
        IModel channel,
        IMessagePublisher<TRevertMessage>? revertMessagePublisher)
        : base(channel)
    {
        _logger = logger;
        _channel = channel;
        _revertMessagePublisher = revertMessagePublisher;
    }

    public override async Task HandleBasicDeliver(
        string consumerTag,
        ulong deliveryTag,
        bool redelivered,
        string exchange,
        string routingKey,
        IBasicProperties properties,
        ReadOnlyMemory<byte> body)
    {
        _logger.LogInformation("Consuming Message");
        _logger.LogInformation(string.Concat("Message received from the exchange ", exchange));
        _logger.LogInformation(string.Concat("Consumer tag: ", consumerTag));
        _logger.LogInformation(string.Concat("Delivery tag: ", deliveryTag));
        _logger.LogInformation(string.Concat("Routing tag: ", routingKey));
        var messageString = Encoding.UTF8.GetString(body.ToArray());
        _logger.LogInformation(string.Concat("Message: ", messageString));
        var message = JsonSerializer.Deserialize<TMessage>(messageString);
        if (message == null)
        {
            _logger.LogError("Message is null");
            return;
        }

        var attemptsCount = GetAttemptsCountFromProps(properties);
        if (attemptsCount is > RetryMaxCount)
        {
            _logger.LogError(
                "Maximum number of retry attempts exceeded for message from exchange {Exchange} and routing key {RoutingKey}",
                exchange, routingKey);
            _channel.BasicAck(deliveryTag, false);
            return;
        }

        try
        {
            await ExecuteEventAsync(message);
            _channel.BasicAck(deliveryTag, false);
        }
        catch (Exception e)
        {
            _logger.LogError(e,"Consuming message for exchange {Exchange} and routing key {RoutingKey} error: {ErrorMessage}",
                exchange, routingKey, e.Message);
            _channel.BasicNack(deliveryTag, false, false);
        }
    }

    public abstract Task ExecuteEventAsync(TMessage message);

    protected async Task RejectEventAsync(TMessage message)
    {
        var rejeectableEvenMessage = message as IRejectableEvent;
        if (rejeectableEvenMessage == null || _revertMessagePublisher == null)
        {
            return;
        }
        _logger.LogInformation(
            "Publish revert message {MessageType} with transaction id {TransactionId}",
            typeof(TMessage).ToString(),
            message.TransactionId);
        if (rejeectableEvenMessage.GetRejectEvent() is TRevertMessage rejectEvent)
        {
            await _revertMessagePublisher.SendAsync(rejectEvent);
        }
    }

    private long? GetAttemptsCountFromProps(IBasicProperties properties)
    {
        var deathProperties = properties.Headers?["x-death"] as List<object>;
        if (deathProperties != null && deathProperties.Any())
        {
            var reasonDictionary = deathProperties.First() as Dictionary<string, object>;
            if (reasonDictionary != null && reasonDictionary.TryGetValue("count", out object? countObj))
            {
                return (long)countObj;
            }
        }

        return null;
    }
}