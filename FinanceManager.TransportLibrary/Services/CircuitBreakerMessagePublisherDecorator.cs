using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using RabbitMQ.Client.Exceptions;

namespace FinanceManager.TransportLibrary.Services;

public class CircuitBreakerMessagePublisherDecorator<TMessage> : IMessagePublisher<TMessage>
{
    private const int ExceptionsAllowedBeforeBreaking = 3;
    private static readonly TimeSpan BlockTimeout = TimeSpan.FromSeconds(10);

    private readonly ILogger<CircuitBreakerMessagePublisherDecorator<TMessage>> _logger;
    private readonly IMessagePublisher<TMessage> _publisher;
    private readonly AsyncCircuitBreakerPolicy _sendMessageBreaker;

    public CircuitBreakerMessagePublisherDecorator(
        ILogger<CircuitBreakerMessagePublisherDecorator<TMessage>> logger,
        IMessagePublisher<TMessage> publisher)
    {
        _logger = logger;
        _publisher = publisher;

        void OnBreak(Exception exception, TimeSpan timeSpan, Context context)
        {
            _logger.LogWarning(
                "Circuit breaker for sending via RabbitMQ was break with error. Message: {ErrorMessage}",
                exception.Message);
        }

        void OnReset(Context _) => _logger.LogInformation("Circuit breaker for sending via RabbitMQ was reset");

        // overflow queue isn't processed by specific exception time, but
        // there are some exceptions or error messages that indicate the queue is full,
        // which would be handled under OperationInterruptedException.
        _sendMessageBreaker = Policy
            .Handle<BrokerUnreachableException>()
            .Or<OperationInterruptedException>()
            .Or<AlreadyClosedException>()
            .Or<ChannelAllocationException>()
            .CircuitBreakerAsync(ExceptionsAllowedBeforeBreaking, BlockTimeout, OnBreak, OnReset);

    }
    public async Task SendAsync(TMessage message)
    {
        if (_sendMessageBreaker.CircuitState == CircuitState.Open)
        {
            _logger.LogWarning("Circuit breaker for sending via RabbitMQ is open, RabbitMQ don't work");
            return;
        }

        await _sendMessageBreaker.ExecuteAsync(async () => await _publisher.SendAsync(message));
    }
}