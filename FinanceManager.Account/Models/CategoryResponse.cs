using FinanceManager.Account.Domain;

namespace FinanceManager.Account.Models;

public class CategoryResponse
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = null!;
    public Guid? ParentId { get; set; }
    public long? UserId { get; set; }
    public CategoryType Type { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Link[] Links { get; set; } = null!;
}