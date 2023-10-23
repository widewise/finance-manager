using RedLockNet;

namespace FinanceManager.File.Services;

public class ImportDataBackgroundService: BackgroundService
{
  private readonly ILogger<ImportDataBackgroundService> _logger;
  private readonly IServiceProvider _serviceProvider;
  private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);
  private const string LockKey = "ImportData";

  public ImportDataBackgroundService(
    ILogger<ImportDataBackgroundService> logger
    , IServiceProvider serviceProvider)
  {
    _logger = logger;
    _serviceProvider = serviceProvider;
  }
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    _logger.LogInformation($"{nameof(ImportDataBackgroundService)} is started");
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
    _logger.LogInformation($"{nameof(ImportDataBackgroundService)} is stopped");
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
          $"{nameof(ImportDataBackgroundService)} execution has already started by another instance");
        return;
      }

      _logger.LogInformation("Importing data is running ...");

      var service = scope.ServiceProvider.GetRequiredService<IImportDataService>();
      await service.ExecuteAsync();
    }
    catch (Exception e)
    {
      _logger.LogError(e,
        "{ServiceName} execution error: {ErrorMessage}",
        nameof(ImportDataBackgroundService),
        e.Message);
    }
  }
}