namespace FinanceManager.Account.Models;

public class UpdateAccountModel
{
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }
    public string? Description { get; set; }
}