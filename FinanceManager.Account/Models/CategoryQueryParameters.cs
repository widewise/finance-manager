namespace FinanceManager.Account.Models;

public class CategoryQueryParameters
{
    public long? UserId { get; set; }
    public Guid? ParentId { get; set; }
    public string? RequestId { get; set; }
}