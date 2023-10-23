namespace FinanceManager.TransportLibrary.Models;

public class CustomAuthenticationSettings
{
    public static string SectionName => "CustomAuthenticationSettings";
    public string Key { get; set; } = null!;
    public string IdentityUrl { get; set; } = null!;
}