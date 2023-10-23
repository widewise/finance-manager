namespace FinanceManager.TransportLibrary.Models;

public class ApiKeySettings
{
    public static string Section => "ApiKeySettings";
    public string ApiKey { get; set; } = null!;
}