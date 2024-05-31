namespace FinanceManager.Account.Models;

public class Link
{
    public string Href { get; }
    public string Rel { get; }
    public string Method { get; }

    public Link(string href, string rel, string method)
    {
        Href = href;
        Rel = rel;
        Method = method;
    }
}