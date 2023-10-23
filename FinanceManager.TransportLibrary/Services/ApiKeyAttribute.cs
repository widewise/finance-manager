using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.TransportLibrary.Services;

public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute()
        : base(typeof(ApiKeyAuthorizationFilter))
    {
    }
}