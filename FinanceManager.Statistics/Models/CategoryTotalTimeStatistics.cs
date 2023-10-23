namespace FinanceManager.Statistics.Models;

public class CategoryTotalTimeStatistics
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public Guid CategoryId { get; set; }
    public decimal TotalValue { get; set; }
    public int? WeekNumberOfYear { get; set; }
    public int? Month { get; set; }
    public int Year { get; set; }
}