using FinanceManager.Events.Models;
using FinanceManager.Expense.Services;
using FinanceManager.TransportLibrary.Models;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.Expense.Consumers;

public class DeleteExpenseConsumer:
    MessageConsumer<DeleteExpenseEvent, IEvent>,
    IMessageConsumer<DeleteExpenseEvent>
{
    private readonly ILogger<MessageConsumer<DeleteExpenseEvent, IEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public DeleteExpenseConsumer(
        ILogger<MessageConsumer<DeleteExpenseEvent, IEvent>> logger,
        IModel channel,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, null)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(DeleteExpenseEvent message)
    {
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IExpenseService>();
            await service.DeleteAsync(message.Id);
        }

        _logger.LogInformation(
            "Delete expense for transaction with id {TransactionId} was preformed",
            message.TransactionId);
    }
}