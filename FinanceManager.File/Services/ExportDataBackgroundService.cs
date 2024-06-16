using RedLockNet;

namespace FinanceManager.File.Services;

public class ExportDataBackgroundService: BackgroundService
{
    private readonly ILogger<ExportDataBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);
    private const string LockKey = "ExportData";

    public ExportDataBackgroundService(
        ILogger<ExportDataBackgroundService> logger
        , IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("{ServiceName} is started", nameof(ExportDataBackgroundService));
        while (!stoppingToken.IsCancellationRequested)
        {
            //TODO: exclude to config
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            await DoWorkAsync();
        }
        _logger.LogInformation("{ServiceName} is stopped", nameof(ExportDataBackgroundService));
    }

    private async Task DoWorkAsync()
    {
        try
        {
            await using var scope = _serviceProvider.CreateAsyncScope();
            var distributedLock = scope.ServiceProvider.GetRequiredService<IDistributedLockFactory>();
            await using var locker = await distributedLock.CreateLockAsync(LockKey, LockTimeout);
            if (!locker.IsAcquired)
            {
                _logger.LogInformation(
                    "{ServiceName} execution has already started by another instance", nameof(ExportDataBackgroundService));
                return;
            }

            _logger.LogInformation("Exporting data is running ...");

            var service = scope.ServiceProvider.GetRequiredService<IExportDataService>();
            await service.ExecuteAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "{ServiceName} execution error: {ErrorMessage}",
                nameof(ExportDataBackgroundService),
                e.Message);
        }
    }
}