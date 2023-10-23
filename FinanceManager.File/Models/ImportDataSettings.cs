namespace FinanceManager.File.Models;

public class ImportDataSettings
{
    public static string Section = "ImportDataSettings";
    public int BatchSize { get; set; }
    public int WaitingUpdatesTimeoutInSeconds { get; set; }
    public int WaitingUpdatesAttempts { get; set; }
}