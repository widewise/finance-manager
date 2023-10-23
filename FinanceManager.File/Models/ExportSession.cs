namespace FinanceManager.File.Models;

public class ExportSession
{
    public long Id { get; set; }
    public string RequestId { get; set; } = null!;
    public long UserId { get; set; }
    public long? FileId { get; set; }
    public DateTime DateTime { get; set; }
    public SessionState State { get; set; }
}