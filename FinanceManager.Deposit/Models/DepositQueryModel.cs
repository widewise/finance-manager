namespace FinanceManager.Deposit.Models;

public class DepositQueryModel
{
    public Guid? Id { get; set; }
    public Guid? AccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime? Date { get; set; }
}