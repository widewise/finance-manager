namespace FinanceManager.File.Models;

public class FileContentItem
{
    public string? FromAccountName { get; set; }
    public string? FromCurrencyName { get; set; }
    public string? FromCategoryName { get; set; }
    public decimal? FromValue { get; set; }
    public string? ToAccountName { get; set; }
    public string? ToCurrencyName { get; set; }
    public string? ToCategoryName { get; set; }
    public decimal? ToValue { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
}