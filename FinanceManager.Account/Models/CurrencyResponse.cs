namespace FinanceManager.Account.Models;

public class CurrencyResponse
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = null!;
    public long UserId { get; set; }
    public string ShortName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }
    public Link[] Links { get; set; } = null!;
}