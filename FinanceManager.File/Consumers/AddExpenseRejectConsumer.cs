using FinanceManager.Events.Models;
using FinanceManager.File.Services;
using FinanceManager.TransportLibrary.Models;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.File.Consumers;

public class AddExpenseRejectConsumer:
    MessageConsumer<AddExpenseRejectEvent, IEvent>,
    IMessageConsumer<AddExpenseRejectEvent>
{
    private readonly ILogger<MessageConsumer<AddExpenseRejectEvent, IEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AddExpenseRejectConsumer(
        ILogger<MessageConsumer<AddExpenseRejectEvent, IEvent>> logger,
        IModel channel,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, null)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(AddExpenseRejectEvent message)
    {
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IImportSessionService>();
            await service.RejectAsync(message.TransactionId);
        }

        _logger.LogInformation(
            "Reject expense for transaction with id {TransactionId} was preformed",
            message.TransactionId);
    }
}