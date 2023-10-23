namespace FinanceManager.File.Models;

public class FinanceManagerSettings
{
    public static string Section = "FinanceManagerSettings";
    public string AccountServiceBaseUrl { get; set; } = null!;
    public string DepositServiceBaseUrl { get; set; } = null!;
    public string ExpenseServiceBaseUrl { get; set; } = null!;
    public string TransferServiceBaseUrl { get; set; } = null!;
}