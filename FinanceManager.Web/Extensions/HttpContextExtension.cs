using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace FinanceManager.Web.Extensions;

public static class HttpContextExtension
{
    public static bool HasUserId(this HttpContext httpContext, out long userId)
    {
        userId = 0;
        var innerUserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(innerUserId?.Value))
        {
            return false;
        }

        return long.TryParse(innerUserId.Value, out userId);
    }

    public static bool HasUserAddress(this HttpContext httpContext, out string userAddress)
    {
        userAddress = string.Empty;
        var innerUserId = httpContext.User.FindFirst(ClaimTypes.Email);
        if (string.IsNullOrEmpty(innerUserId?.Value))
        {
            return false;
        }

        userAddress = innerUserId.Value;
        return true;
    }
}