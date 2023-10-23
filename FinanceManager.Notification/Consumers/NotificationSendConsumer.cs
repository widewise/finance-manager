using FinanceManager.Events.Models;
using FinanceManager.Notification.Models;
using FinanceManager.TransportLibrary.Models;
using FinanceManager.TransportLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace FinanceManager.Notification.Consumers;

public class NotificationSendConsumer :
    MessageConsumer<NotificationSendEvent, IEvent>,
    IMessageConsumer<NotificationSendEvent>
{
    private readonly ILogger<MessageConsumer<NotificationSendEvent, IEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly NotificationSettings _settings;

    public NotificationSendConsumer(
        ILogger<MessageConsumer<NotificationSendEvent, IEvent>> logger,
        IModel channel,
        IServiceScopeFactory scopeFactory,
        IOptions<NotificationSettings> options)
        : base(logger, channel, null)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _settings = options.Value;
    }

    public override async Task ExecuteEventAsync(NotificationSendEvent message)
    {
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (await dbContext.Notifications.AnyAsync(x => x.TransactionId == message.TransactionId))
            {
                _logger.LogWarning(
                    "Notification has already created for request with id {RequestId}",
                    message.TransactionId);
            }
            else
            {
                //TODO: emulate send via email, telegram and etc.
                await dbContext.AddAsync(new Models.Notification
                {
                    TransactionId = message.TransactionId,
                    Type = message.Type,
                    ToAddress = message.ToAddress,
                    FromAddress = _settings.FromAddress,
                    Title = message.Title,
                    Body = message.Body
                });
                await dbContext.SaveChangesAsync();
            }
        }

        _logger.LogInformation(
            "Notification send for transaction with id {TransactionId} was preformed",
            message.TransactionId);
    }
}