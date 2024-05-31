using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Web.Services;

public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute()
        : base(typeof(ApiKeyAuthorizationFilter))
    {
    }
}