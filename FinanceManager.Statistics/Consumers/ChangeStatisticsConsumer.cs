using FinanceManager.Events.Models;
using FinanceManager.Statistics.Services;
using FinanceManager.TransportLibrary.Models;
using FinanceManager.TransportLibrary.Services;
using RabbitMQ.Client;

namespace FinanceManager.Statistics.Consumers;

public class ChangeStatisticsConsumer :
    MessageConsumer<ChangeStatisticsEvent, IEvent>,
    IMessageConsumer<ChangeStatisticsEvent>
{
    private readonly ILogger<MessageConsumer<ChangeStatisticsEvent, IEvent>> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ChangeStatisticsConsumer(
        ILogger<MessageConsumer<ChangeStatisticsEvent, IEvent>> logger,
        IModel channel,
        IServiceScopeFactory scopeFactory)
        : base(logger, channel, null)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public override async Task ExecuteEventAsync(ChangeStatisticsEvent message)
    {
        await using (var scope = _scopeFactory.CreateAsyncScope())
        {
            var balanceLevelStatisticsService =
                scope.ServiceProvider.GetRequiredService<IBalanceLevelStatisticsService>();
            await balanceLevelStatisticsService.UpdateStatisticsAsync(
                message.AccountId,
                message.Value,
                message.Date);

            var categoryTotalTimeStatisticsService =
                scope.ServiceProvider.GetRequiredService<ICategoryTotalTimeStatisticsService>();
            await categoryTotalTimeStatisticsService.UpdateStatisticsAsync(
                message.AccountId,
                message.CategoryId,
                message.Value,
                message.Date);
        }

        _logger.LogInformation(
            "Change statistics for transaction with id {TransactionId} was preformed",
            message.TransactionId);
    }
}