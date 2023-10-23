using FinanceManager.TransportLibrary;
using IdentityServer4.Models;

namespace FinanceManager.User;


internal class Resources
{
    public static IEnumerable<IdentityResource> GetIdentityResources()
    {
        return new[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource
            {
                Name = "role",
                UserClaims = new List<string> {"role"}
            }
        };
    }

    public static IEnumerable<ApiResource> GetApiResources()
    {
        var secrets = new List<Secret> { new("!SomethingSecret!".Sha256()) };
        var scopes = new List<string> { "api1.read", "api1.write" };
        //UserClaims = new List<string> {"role"}
        return new[]
        {
            new ApiResource
            {
                Name = FinanceIdentityConstants.AccountAudience,
                DisplayName = FinanceIdentityConstants.AccountTitle,
                Description = FinanceIdentityConstants.AccountTitle,
                Scopes = scopes,
                ApiSecrets = secrets
            },
            new ApiResource
            {
                Name = FinanceIdentityConstants.DepositAudience,
                DisplayName = FinanceIdentityConstants.DepositTitle,
                Description = FinanceIdentityConstants.DepositTitle,
                Scopes = scopes,
                ApiSecrets = secrets
            },
            new ApiResource
            {
                Name = FinanceIdentityConstants.ExpenseAudience,
                DisplayName = FinanceIdentityConstants.ExpenseTitle,
                Description = FinanceIdentityConstants.ExpenseTitle,
                Scopes = scopes,
                ApiSecrets = secrets
            },
            new ApiResource
            {
                Name = FinanceIdentityConstants.TransferAudience,
                DisplayName = FinanceIdentityConstants.TransferTitle,
                Description = FinanceIdentityConstants.TransferTitle,
                Scopes = scopes,
                ApiSecrets = secrets
            },
            new ApiResource
            {
                Name = FinanceIdentityConstants.FileAudience,
                DisplayName = FinanceIdentityConstants.FileTitle,
                Description = FinanceIdentityConstants.FileTitle,
                Scopes = scopes,
                ApiSecrets = secrets
            },
        };
    }

    public static IEnumerable<ApiScope> GetApiScopes()
    {
        return new[]
        {
            new ApiScope("api1.read", "Read Access to API #1"),
            new ApiScope("api1.write", "Write Access to API #1")
        };
    }
}