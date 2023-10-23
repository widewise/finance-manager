namespace FinanceManager.Expense.Models;

public class CreateUpdateExpenseModel
{
    public Guid? Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}