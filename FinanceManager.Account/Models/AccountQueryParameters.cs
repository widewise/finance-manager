namespace FinanceManager.Account.Models;

public class AccountQueryParameters
{
    public Guid? Id { get; set; }
    public long? UserId { get; set; }
    public string? RequestId { get; set; }
}