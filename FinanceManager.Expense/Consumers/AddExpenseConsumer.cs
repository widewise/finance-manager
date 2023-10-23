using FinanceManager.Events.Models;
using FinanceManager.Expense.Models;
using FinanceManager.Expense.Services;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.Expense.Consumers;

public class AddExpenseConsumer:
    MessageConsumer<AddExpenseEvent, AddExpenseRejectEvent>,
    IMessageConsumer<AddExpenseEvent>
{
    private readonly ILogger<MessageConsumer<AddExpenseEvent, AddExpenseRejectEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public AddExpenseConsumer(
        ILogger<MessageConsumer<AddExpenseEvent, AddExpenseRejectEvent>> logger,
        IModel channel,
        IMessagePublisher<AddExpenseRejectEvent>? revertMessagePublisher,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, revertMessagePublisher)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(AddExpenseEvent message)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var service = scope.ServiceProvider.GetRequiredService<IExpenseService>();
        var expense = await service.CreateAsync(
            message.TransactionId,
            message.UserId,
            message.UserAddress,
            new CreateUpdateExpenseModel
            {
                Id = message.Id,
                AccountId = message.AccountId,
                CategoryId = message.CategoryId,
                Value = message.Value,
                Date = message.Date
            });
        if (expense == null)
        {
            await RejectEventAsync(message);
            return;
        }

        _logger.LogInformation(
            "Create expense with transaction id {TransactionId} was preformed",
            message.TransactionId);
    }
}