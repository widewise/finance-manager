﻿using FinanceManager.TransportLibrary.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FinanceManager.TransportLibrary.Services.Outbox;

public interface IOutboxSessionService
{
    Task ExecuteAsync();
}

public class OutboxSessionService : IOutboxSessionService
{
    private readonly ILogger<OutboxSessionService> _logger;
    private readonly OutboxSettings _settings;
    private readonly IMessageRepository _messageRepository;
    private readonly IServiceProvider _serviceProvider;

    public OutboxSessionService(
        ILogger<OutboxSessionService> logger,
        OutboxSettings settings,
        IMessageRepository messageRepository,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _settings = settings;
        _messageRepository = messageRepository;
        _serviceProvider = serviceProvider;
    }

    public async Task ExecuteAsync()
    {
        var messages = await _messageRepository.GetRecentAsync(_settings.BatchSize);
        foreach (var outboxMessage in messages)
        {
            var tMessage = Type.GetType(outboxMessage.Type);
            if (tMessage == null)
            {
                _logger.LogError("Type of message isn't recognized");
                throw new ArgumentNullException(nameof(tMessage));
            }
            var message = outboxMessage.Content != null ? JsonConvert.DeserializeObject(outboxMessage.Content, tMessage) : null;
            var publisherType  = typeof(MessagePublisher<>);
            var genericPublisherType = publisherType.MakeGenericType(tMessage);
            if (_serviceProvider.GetRequiredService(genericPublisherType) is IMessagePublisher publisher)
            {
                try
                {
                    await publisher.SendAsync(message ?? new object());
                    outboxMessage.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Outbox message sending error: {ErrorMessage}", e.Message);
                    outboxMessage.Error = e.Message;
                }
                finally
                {
                    await _messageRepository.UpdateAsync(outboxMessage);
                }
            }
        }
    }
}