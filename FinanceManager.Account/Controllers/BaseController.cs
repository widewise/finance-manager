using System.Reflection;
using FinanceManager.TransportLibrary.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.Account.Controllers;

[ApiController]
public class BaseController : ControllerBase
{
    protected void AddAllowHeader()
    {
        HttpContext.Response.Headers.Add("Allow", GetSupportedHttpMethods());
    }

    private string GetSupportedHttpMethods()
    {
        var methods = new List<string> { "OPTIONS" };
        var userIsAuthorized = User.Identity?.IsAuthenticated ?? false;

        var currentControllerType = GetType();
        var hasDefaultAuth = currentControllerType.GetCustomAttributes().Any(a => a is AuthorizeAttribute);
        var methodInfos =  currentControllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        foreach (var methodInfo in methodInfos)
        {
            var method = GetHttpMethod(methodInfo, userIsAuthorized, hasDefaultAuth);
            if (!string.IsNullOrEmpty(method))
            {
                methods.Add(method);
            }
        }

        return string.Join(", ", methods.Distinct());
    }

    private string GetHttpMethod(MethodInfo methodInfo, bool userIsAuthorized, bool hasDefaultAuth)
    {
        var attributes = methodInfo.GetCustomAttributes().ToArray();
        if (!MethodAuthCheck(attributes, userIsAuthorized, hasDefaultAuth))
        {
            return string.Empty;
        }

        foreach (var attribute in attributes)
        {
            if(attribute is HttpGetAttribute)
            {
                return "GET";
            }
            if(attribute is HttpPostAttribute)
            {
                return "POST";
            }
            if(attribute is HttpPutAttribute)
            {
                return "PUT";
            }
            if(attribute is HttpPatchAttribute)
            {
                return "PATCH";
            }
            if(attribute is HttpDeleteAttribute)
            {
                return "DELETE";
            }
        }
        return string.Empty;
    }

    private bool MethodAuthCheck(IEnumerable<Attribute> attributes, bool userIsAuthorized, bool hasDefaultAuth)
    {
        if (userIsAuthorized)
        {
            return true;
        }

        if (hasDefaultAuth)
        {
            return attributes.Any(a => a is AllowAnonymousAttribute);
        }

        return !attributes.Any(a => a is AuthorizeAttribute | a is ApiKeyAttribute);
    }
}