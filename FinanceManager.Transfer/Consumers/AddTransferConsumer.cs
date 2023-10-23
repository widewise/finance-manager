using FinanceManager.Events.Models;
using FinanceManager.Transfer.Models;
using FinanceManager.Transfer.Services;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.Transfer.Consumers;

public class AddTransferConsumer:
    MessageConsumer<AddTransferEvent, AddTransferRejectEvent>,
    IMessageConsumer<AddTransferEvent>
{
    private readonly ILogger<MessageConsumer<AddTransferEvent, AddTransferRejectEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AddTransferConsumer(
        ILogger<MessageConsumer<AddTransferEvent, AddTransferRejectEvent>> logger,
        IModel channel,
        IMessagePublisher<AddTransferRejectEvent>? revertMessagePublisher,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, revertMessagePublisher)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(AddTransferEvent message)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<ITransferService>();
        var transfer = await service.CreateAsync(
            message.TransactionId,
            message.UserId,
            message.UserAddress,
            new CreateTransferModel
            {
                Id = message.Id,
                FromAccountId = message.FromAccountId,
                ToAccountId = message.ToAccountId,
                FromValue = message.FromValue,
                ToValue = message.ToValue,
                Date = message.Date,
                Description = message.Description
            });
        if (transfer == null)
        {
            await RejectEventAsync(message);
            return;
        }

        _logger.LogInformation(
            "Create transfer with transaction id {TransactionId} was preformed",
            message.TransactionId);
    }
}