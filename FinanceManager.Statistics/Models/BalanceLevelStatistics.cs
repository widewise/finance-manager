namespace FinanceManager.Statistics.Models;

public class BalanceLevelStatistics
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
    public DateTime Date { get; set; }
}