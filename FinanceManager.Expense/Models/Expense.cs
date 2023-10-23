namespace FinanceManager.Expense.Models;

public class Expense
{
    public Guid Id { get; set; }
    public string RequestId { get; set; } = null!;
    public long UserId { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}