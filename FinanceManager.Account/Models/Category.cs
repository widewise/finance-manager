namespace FinanceManager.Account.Models;

public class Category
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public long? UserId { get; set; }
    public CategoryType Type { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public enum CategoryType
{
    Deposit = 0,
    Expense = 1,
    Transfer = 2
}