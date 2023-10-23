namespace FinanceManager.Statistics.Models;

public class CategoryStatisticsRequest
{
    public StatisticsTimeType TimeType { get; set; }
    public int? Year { get; set; }
}