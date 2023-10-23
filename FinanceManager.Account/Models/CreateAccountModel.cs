namespace FinanceManager.Account.Models;

public class CreateAccountModel
{
    public string Name { get; set; } = null!;
    public Guid CurrencyId { get; set; }
    public string? Icon { get; set; }
    public string? Description { get; set; }
}