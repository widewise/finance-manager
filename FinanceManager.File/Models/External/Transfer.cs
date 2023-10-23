namespace FinanceManager.File.Models.External;

public class Transfer
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = null!;
    public long UserId { get; set; }
    public Guid DepositId { get; set; }
    public Guid ExpenseId { get; set; }
    public string? Description { get; set; }
}