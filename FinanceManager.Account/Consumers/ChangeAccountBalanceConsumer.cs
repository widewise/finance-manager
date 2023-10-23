using FinanceManager.Account.Models;
using FinanceManager.Account.Services;
using FinanceManager.Events.Models;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.Account.Consumers;

public class ChangeAccountBalanceConsumer:
    MessageConsumer<ChangeAccountBalanceEvent, ChangeAccountBalanceRejectEvent>,
    IMessageConsumer<ChangeAccountBalanceEvent>
{
    private readonly ILogger<MessageConsumer<ChangeAccountBalanceEvent, ChangeAccountBalanceRejectEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ChangeAccountBalanceConsumer(
        ILogger<MessageConsumer<ChangeAccountBalanceEvent, ChangeAccountBalanceRejectEvent>> logger,
        IModel channel,
        IMessagePublisher<ChangeAccountBalanceRejectEvent>? revertMessagePublisher,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, revertMessagePublisher)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(ChangeAccountBalanceEvent message)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var accountService = scope.ServiceProvider.GetRequiredService<IAccountBalanceService>();
        if (!await accountService.UpdateAccountBalanceAsync(new UpdateAccountBalanceModel
            {
                TransactionId = message.TransactionId,
                AccountId = message.AccountId,
                CategoryId = message.CategoryId,
                Value = message.Value,
                Date = message.Date,
                UserAddress = message.UserAddress
            }))
        {
            await RejectEventAsync(message);
            return;
        }

        _logger.LogInformation(
            "Account balance with id {Account} has been changed by message with transaction id {TransactionId} was preformed",
            message.AccountId,
            message.TransactionId);
    }
}