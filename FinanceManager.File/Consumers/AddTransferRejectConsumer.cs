using FinanceManager.Events.Models;
using FinanceManager.File.Services;
using FinanceManager.TransportLibrary.Models;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.File.Consumers;

public class AddTransferRejectConsumer:
    MessageConsumer<AddTransferRejectEvent, IEvent>,
    IMessageConsumer<AddTransferRejectEvent>
{
    private readonly ILogger<MessageConsumer<AddTransferRejectEvent, IEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AddTransferRejectConsumer(
        ILogger<MessageConsumer<AddTransferRejectEvent, IEvent>> logger,
        IModel channel,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, null)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(AddTransferRejectEvent message)
    {
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IImportSessionService>();
            await service.RejectAsync(message.TransactionId);
        }

        _logger.LogInformation(
            "Reject transfer for transaction with id {TransactionId} was preformed",
            message.TransactionId);
    }
}