using FinanceManager.Web.Models;

namespace FinanceManager.Web.Services;

public interface IApiKeyValidator
{
    bool IsValid(string apiKey);
}

public class ApiKeyValidator : IApiKeyValidator
{
    private readonly ApiKeySettings _settings;

    public ApiKeyValidator(
        ApiKeySettings settings)
    {
        _settings = settings;
    }
    public bool IsValid(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
        {
            return false;
        }

        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            return false;
        }

        if (!string.Equals(_settings.ApiKey, apiKey, StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        return true;
    }
}