using FinanceManager.Account.Models;
using FinanceManager.Account.Services;
using FinanceManager.Events.Models;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.Account.Consumers;

public class TransferBetweenAccountsConsumer:
    MessageConsumer<TransferBetweenAccountsEvent, TransferBetweenAccountsRejectEvent>,
    IMessageConsumer<TransferBetweenAccountsEvent>
{
    private readonly ILogger<MessageConsumer<TransferBetweenAccountsEvent, TransferBetweenAccountsRejectEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TransferBetweenAccountsConsumer(
        ILogger<MessageConsumer<TransferBetweenAccountsEvent, TransferBetweenAccountsRejectEvent>> logger,
        IModel channel,
        IMessagePublisher<TransferBetweenAccountsRejectEvent>? revertMessagePublisher,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, revertMessagePublisher)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(TransferBetweenAccountsEvent message)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IAccountBalanceService>();
        if (!await service.TransferBetweenAccountsAsync(new TransferBetweenAccountsModel
            {
                TransactionId = message.TransactionId,
                FromAccountId = message.FromAccountId,
                ToAccountId = message.ToAccountId,
                FromValue = message.FromValue,
                ToValue = message.ToValue,
                Date = message.Date,
                UserAddress = message.UserAddress
            }))
        {
            await RejectEventAsync(message);
            return;
        }

        _logger.LogInformation(
            "Transfer from account with id {FromAccountIf} to account with id {ToAccountId} by message with transaction id {TransactionId} was preformed",
            message.FromAccountId,
            message.ToAccountId,
            message.TransactionId);
    }
}