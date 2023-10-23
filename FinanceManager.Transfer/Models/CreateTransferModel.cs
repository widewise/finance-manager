namespace FinanceManager.Transfer.Models;

public class CreateTransferModel
{
    public Guid? Id { get; set; }
    public Guid FromAccountId { get; set; }
    public Guid ToAccountId { get; set; }
    public DateTime Date { get; set; }
    public decimal FromValue { get; set; }
    public decimal ToValue { get; set; }
    public string? Description { get; set; }
}