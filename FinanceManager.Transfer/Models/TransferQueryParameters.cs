namespace FinanceManager.Transfer.Models;

public class TransferQueryParameters
{
    public Guid? Id { get; set; }
    public string? RequestId { get; set; }
    public long? UserId { get; set; }
}