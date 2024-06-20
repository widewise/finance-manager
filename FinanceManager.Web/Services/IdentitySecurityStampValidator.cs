using FinanceManager.Web.Models;

namespace FinanceManager.Web.Services;

public interface IIdentitySecurityStampValidator
{
    Task<SecurityStampValidatorResult> ValidateAsync(string? userName, string? securityStamp);
}

public class IdentitySecurityStampValidator : IIdentitySecurityStampValidator
{
    private readonly HttpClient _httpClient;

    public IdentitySecurityStampValidator(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SecurityStampValidatorResult> ValidateAsync(string? userName, string? securityStamp)
    {
        if (string.IsNullOrEmpty(securityStamp))
        {
            return new SecurityStampValidatorResult(false, "Token security stamp is empty");
        }

        if (string.IsNullOrEmpty(userName))
        {
            return new SecurityStampValidatorResult(false, "Token user name is empty");
        }
                        
        var response = await _httpClient.GetAsync($"api/v1/auth/verifySecurityStamp?userName={userName}&securityStamp={securityStamp}");

        if (!response.IsSuccessStatusCode)
        {
            return new SecurityStampValidatorResult(false, "Call to identity service error");
        }

        var isValidString = await response.Content.ReadAsStringAsync();
        var isValid = bool.Parse(isValidString);
    
        if (!isValid)
        {
            return new SecurityStampValidatorResult(false, "Invalid Security Stamp");
        }

        return new SecurityStampValidatorResult(true);
    }
}