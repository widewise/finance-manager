namespace FinanceManager.File.Models;

public class ImportDataSettings
{
    public const string Section = "ImportDataSettings";
    public int BatchSize { get; set; }
    public int WaitingUpdatesTimeoutInSeconds { get; set; }
    public int WaitingUpdatesAttempts { get; set; }
}