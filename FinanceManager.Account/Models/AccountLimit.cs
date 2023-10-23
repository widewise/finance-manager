namespace FinanceManager.Account.Models;

public class AccountLimit
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = null!;
    public Guid AccountId { get; set; }
    public decimal LimitValue { get; set; }
    public AccountLimitType Type { get; set; }
    public AccountLimitTime Time { get; set; }
    public string? Description { get; set; }
}

public enum AccountLimitType
{
    Notify = 0,
    Restrict = 1,
}

public enum AccountLimitTime
{
    Week = 0,
    Month = 1,
    Year = 2,
}