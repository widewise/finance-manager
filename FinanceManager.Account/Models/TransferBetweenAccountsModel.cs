namespace FinanceManager.Account.Models;

public class TransferBetweenAccountsModel
{
    public string TransactionId { get; set; } = null!;
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public DateTime Date { get; set; }
    public decimal FromValue { get; set; }
    public decimal ToValue { get; set; }
    public string? UserAddress { get; set; }
}