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
        _logger.LogInformation($"{nameof(OutboxBackgroundService)} is started");
        while (!stoppingToken.IsCancellationRequested)
        {
            var getWaitTimeService = _serviceProvider.GetRequiredService<IGetWaitTimeForNextOutboxCallService>();

            var waitTime = getWaitTimeService.GetWaitTime();
            _logger.LogInformation(
                $"Outbox trigger will be started in {waitTime.Days} days, {waitTime.Hours} hours, {waitTime.Minutes} minutes");
            await Task.Delay(waitTime, stoppingToken);
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            await DoWorkAsync();
        }
        _logger.LogInformation($"{nameof(OutboxBackgroundService)} is stopped");
    }

    private async Task DoWorkAsync()
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var settings = scope.ServiceProvider.GetRequiredService<OutboxSettings>();
            if (!settings.Enabled)
            {
                _logger.LogInformation($"{nameof(OutboxBackgroundService)} is turned off");
            }
            var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLockFactory>();
            await using var locker = await distributedLock.CreateLockAsync(LockKey, LockTimeout);
            if (!locker.IsAcquired)
            {
                _logger.LogInformation(
                    $"{nameof(OutboxBackgroundService)} execution has already started by another instance");
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