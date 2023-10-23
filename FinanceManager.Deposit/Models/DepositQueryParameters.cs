namespace FinanceManager.Deposit.Models;

public class DepositQueryParameters
{
    public Guid? Id { get; set; }
    public long? UserId { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime? Date { get; set; }
    public string? RequestId { get; set; }
}