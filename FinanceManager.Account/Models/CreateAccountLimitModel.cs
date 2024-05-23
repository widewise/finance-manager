using FinanceManager.Account.Domain;

namespace FinanceManager.Account.Models;

public class CreateAccountLimitModel
{
    public Guid AccountId { get; set; }
    public decimal LimitValue { get; set; }
    public AccountLimitType Type { get; set; }
    public AccountLimitTime Time { get; set; }
    public string? Description { get; set; }
}