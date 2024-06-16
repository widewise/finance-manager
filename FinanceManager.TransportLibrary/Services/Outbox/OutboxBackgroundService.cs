using FinanceManager.TransportLibrary.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RedLockNet;

namespace FinanceManager.TransportLibrary.Services.Outbox;

public class OutboxBackgroundService: BackgroundService
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);
    private const string LockKey = "OutboxTrigger";
    private readonly ILogger<OutboxBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public OutboxBackgroundService(
        ILogger<OutboxBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{ServiceName} is started", nameof(OutboxBackgroundService));
        while (!stoppingToken.IsCancellationRequested)
        {
            var getWaitTimeService = _serviceProvider.GetRequiredService<IGetWaitTimeForNextOutboxCallService>();

            var waitTime = getWaitTimeService.GetWaitTime();
            _logger.LogInformation(
                "Outbox trigger will be started in {Days} days, {Hours} hours, {Minutes} minutes",
                waitTime.Days,
                waitTime.Hours,
                waitTime.Minutes);
            await Task.Delay(waitTime, stoppingToken);
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            await DoWorkAsync();
        }
        _logger.LogInformation("{ServiceName} is stopped", nameof(OutboxBackgroundService));
    }

    private async Task DoWorkAsync()
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var settings = scope.ServiceProvider.GetRequiredService<OutboxSettings>();
            if (!settings.Enabled)
            {
                _logger.LogInformation("{ServiceName} is turned off", nameof(OutboxBackgroundService));
            }
            var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLockFactory>();
            await using var locker = await distributedLock.CreateLockAsync(LockKey, LockTimeout);
            if (!locker.IsAcquired)
            {
                _logger.LogInformation(
                    "{ServiceName} execution has already started by another instance", nameof(OutboxBackgroundService));
                return;
            }

            _logger.LogInformation("Outbox trigger is running ...");

            var sessionService = scope.ServiceProvider.GetRequiredService<IOutboxSessionService>();
            await sessionService.ExecuteAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "{ServiceName} execution error: {ErrorMessage}",
                nameof(OutboxBackgroundService),
                e.Message);
        }
    }
}