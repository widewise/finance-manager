namespace FinanceManager.Statistics.Models;

public class CategoryStatisticsParameters
{
    public Guid AccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public StatisticsTimeType TimeType { get; set; }
    public int? Year { get; set; }
}