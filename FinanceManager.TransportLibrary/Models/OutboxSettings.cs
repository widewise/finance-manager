namespace FinanceManager.TransportLibrary.Models;

public class OutboxSettings
{
    public static string SectionName => "OutboxSettings";
    public bool Enabled { get; set; }
    public int BatchSize { get; set; }
    public string CronSchedule { get; set; } = default!;
}