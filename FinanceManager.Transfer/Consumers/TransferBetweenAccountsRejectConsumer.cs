using FinanceManager.Events.Models;
using FinanceManager.Transfer.Services;
using FinanceManager.TransportLibrary.Models;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.Transfer.Consumers;

public class TransferBetweenAccountsRejectConsumer:
    MessageConsumer<TransferBetweenAccountsRejectEvent, IEvent>,
    IMessageConsumer<TransferBetweenAccountsRejectEvent>
{
    private readonly ILogger<MessageConsumer<TransferBetweenAccountsRejectEvent, IEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TransferBetweenAccountsRejectConsumer(
        ILogger<MessageConsumer<TransferBetweenAccountsRejectEvent, IEvent>> logger,
        IModel channel,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, null)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(TransferBetweenAccountsRejectEvent message)
    {
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<ITransferService>();
            await service.RejectAsync(message.TransactionId);
        }

        _logger.LogInformation(
            "Reject transfer for transaction with id {TransactionId} was preformed",
            message.TransactionId);
    }
}