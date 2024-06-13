namespace FinanceManager.TransportLibrary.Models;

public class Message
{
    public Guid Id { get; set; }
    public string Type { get; set; } = default!;
    public string? Content { get; set; }
    public DateTime OccuredAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public int AttemptCount { get; set; }
    public string? Error { get; set; }
}