namespace FinanceManager.File.Models.External;

public class CreateCurrencyModel
{
    public string ShortName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }
}