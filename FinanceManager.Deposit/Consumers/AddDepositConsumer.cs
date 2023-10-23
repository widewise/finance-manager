using FinanceManager.Deposit.Models;
using FinanceManager.Deposit.Services;
using FinanceManager.Events.Models;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.Deposit.Consumers;

public class AddDepositConsumer:
    MessageConsumer<AddDepositEvent, AddDepositRejectEvent>,
    IMessageConsumer<AddDepositEvent>
{
    private readonly ILogger<MessageConsumer<AddDepositEvent, AddDepositRejectEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AddDepositConsumer(
        ILogger<MessageConsumer<AddDepositEvent, AddDepositRejectEvent>> logger,
        IModel channel,
        IMessagePublisher<AddDepositRejectEvent>? revertMessagePublisher,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, revertMessagePublisher)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(AddDepositEvent message)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IDepositService>();
        var deposit = await service.CreateAsync(
            message.TransactionId,
            message.UserId,
            message.UserAddress,
            new CreateUpdateDepositModel
            {
                Id = message.Id,
                AccountId = message.AccountId,
                CategoryId = message.CategoryId,
                Value = message.Value,
                Date = message.Date
            });
        if (deposit == null)
        {
            await RejectEventAsync(message);
            return;
        }

        _logger.LogInformation(
            "Create deposit with transaction id {TransactionId} was preformed",
            message.TransactionId);
    }
}