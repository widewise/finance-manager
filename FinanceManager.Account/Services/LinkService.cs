using FinanceManager.Account.Models;

namespace FinanceManager.Account.Services;

public interface ILinkService
{
    Link Generate(string endpointName, object? routeValues, string rel, string method);
}
    

public class LinkService : ILinkService
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LinkService(
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public Link Generate(string endpointName, object? routeValues, string rel, string method)
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new NullReferenceException(nameof(_httpContextAccessor.HttpContext));
        }

        var href = _linkGenerator.GetUriByName(_httpContextAccessor.HttpContext, endpointName, routeValues);
        if (string.IsNullOrEmpty(href))
        {
            throw new ArgumentException(nameof(href));
        }

        return new Link(href, rel, method);
    }
}