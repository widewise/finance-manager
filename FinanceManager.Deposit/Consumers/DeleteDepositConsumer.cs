using FinanceManager.Deposit.Services;
using FinanceManager.Events.Models;
using FinanceManager.TransportLibrary.Models;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.Deposit.Consumers;

public class DeleteDepositConsumer:
    MessageConsumer<DeleteDepositEvent, IEvent>,
    IMessageConsumer<DeleteDepositEvent>
{
    private readonly ILogger<MessageConsumer<DeleteDepositEvent, IEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public DeleteDepositConsumer(
        ILogger<MessageConsumer<DeleteDepositEvent, IEvent>> logger,
        IModel channel,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, null)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(DeleteDepositEvent message)
    {
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IDepositService>();
            await service.DeleteAsync(message.Id);
        }

        _logger.LogInformation(
            "Delete deposit for transaction with id {TransactionId} was preformed",
            message.TransactionId);
    }
}