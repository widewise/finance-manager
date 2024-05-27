using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FinanceManager.TransportLibrary.Services.Outbox;

public class OutboxMessagePublisherDecorator<TMessage> : IMessagePublisher<TMessage>
{
    private readonly ILogger<OutboxMessagePublisherDecorator<TMessage>> _logger;
    private readonly IMessagePublisher<TMessage> _publisher;
    private readonly IMessageRepository _messageRepository;

    public OutboxMessagePublisherDecorator(
        ILogger<OutboxMessagePublisherDecorator<TMessage>> logger,
        IMessagePublisher<TMessage> publisher,
        IMessageRepository messageRepository)
    {
        _logger = logger;
        _publisher = publisher;
        _messageRepository = messageRepository;
    }

    
    public async Task SendAsync(TMessage message)
    {
        try
        {
            await _publisher.SendAsync(message);
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                "Error of sending {TMessage} message: {ErrorMessage}. Saving message for later sending",
                typeof(TMessage).Name,
                e.Message);
            await _messageRepository.AddAsync(
                $"{typeof(TMessage).FullName}, {typeof(TMessage).Assembly.FullName}",
                message != null ? JsonConvert.SerializeObject(message) : null);
        }
    }
}