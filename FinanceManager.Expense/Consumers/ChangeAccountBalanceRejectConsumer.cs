﻿using FinanceManager.Events.Models;
using FinanceManager.Expense.Services;
using FinanceManager.TransportLibrary.Models;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.Expense.Consumers;

public class ChangeAccountBalanceRejectConsumer:
    MessageConsumer<ChangeAccountBalanceRejectEvent, IEvent>,
    IMessageConsumer<ChangeAccountBalanceRejectEvent>
{
    private readonly ILogger<MessageConsumer<ChangeAccountBalanceRejectEvent, IEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ChangeAccountBalanceRejectConsumer(
        ILogger<MessageConsumer<ChangeAccountBalanceRejectEvent, IEvent>> logger,
        IModel channel,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, null)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(ChangeAccountBalanceRejectEvent message)
    {
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var service = scope.ServiceProvider.GetRequiredService<IExpenseService>();
            await service.RejectAsync(message.TransactionId);
        }

        _logger.LogInformation(
            "Reject expense for transaction with id {TransactionId} was preformed",
            message.TransactionId);
    }
}