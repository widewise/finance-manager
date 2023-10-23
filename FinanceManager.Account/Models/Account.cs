namespace FinanceManager.Account.Models;

public class Account
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = null!;
    public long UserId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Balance { get; set; }
    public Guid CurrencyId { get; set; }
    public string? Icon { get; set; }
    public string? Description { get; set; }
}