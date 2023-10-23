namespace FinanceManager.Account.Models;

public class UpdateAccountBalanceModel
{
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public string? UserAddress { get; set; }
    public string TransactionId { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}