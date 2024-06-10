namespace FinanceManager.Web;

public static class AuthPolicies
{
    public const string RequireAdmin = "RequireAdmin";
    public const string RequireUser = "RequireUser";

    public const string RequireCategoryRead = "RequireCategoryRead";
    public const string RequireCategoryWrite = "RequireCategoryWrite";
    public const string RequireCurrencyRead = "RequireCurrencyRead";
    public const string RequireCurrencyWrite = "RequireCurrencyWrite";
}