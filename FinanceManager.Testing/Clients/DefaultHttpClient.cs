using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using FinanceManager.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace FinanceManager.Testing.Clients;

public interface IHttpClient
{
    Task<HttpResponseMessage> GetAsync(
        string uri,
        string? authorization = null,
        string? requestId = null,
        string? apiKey = null);

    Task<HttpResponseMessage> PostAsync<T>(
        string uri,
        T content,
        string? authorization = null,
        string? requestId = null,
        string? apiKey = null);

    Task<HttpResponseMessage> PutAsync<T>(
        string uri,
        T content,
        string? authorization = null,
        string? requestId = null,
        string? apiKey = null);

    Task<HttpResponseMessage> DeleteAsync(
        string uri,
        string? authorization = null,
        string? requestId = null,
        string? apiKey = null);
}

[ExcludeFromCodeCoverage]
internal class DefaultHttpClient : IHttpClient
{
    private const string MediaType = "application/json";

    private readonly HttpClient _httpClient;

    public DefaultHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public Task<HttpResponseMessage> GetAsync(
        string uri,
        string? authorization = null,
        string? requestId = null,
        string? apiKey = null)
    {
        return SendWithoutContentAsync(HttpMethod.Get, uri, authorization, requestId, apiKey);
    }

    public Task<HttpResponseMessage> PostAsync<T>(
        string uri,
        T content,
        string? authorization = null,
        string? requestId = null,
        string? apiKey = null)
    {
        return SendWithContentAsync(HttpMethod.Post, uri, content, authorization, requestId, apiKey);
    }

    public Task<HttpResponseMessage> PutAsync<T>(
        string uri,
        T content,
        string? authorization = null,
        string? requestId = null,
        string? apiKey = null)
    {
        return SendWithContentAsync(HttpMethod.Put, uri, content, authorization, requestId, apiKey);
    }

    public Task<HttpResponseMessage> DeleteAsync(
        string uri,
        string? authorization = null,
        string? requestId = null,
        string? apiKey = null)
    {
        return SendWithoutContentAsync(HttpMethod.Delete, uri, authorization, requestId, apiKey);
    }

    private static HttpContent GetHttpContent<TContent>(TContent content)
    {
        var settings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        var serializedContent = JsonConvert.SerializeObject(content, settings);

        return new StringContent(serializedContent, Encoding.UTF8, MediaType);
    }

    private Task<HttpResponseMessage> SendWithoutContentAsync(
        HttpMethod method,
        string uri,
        string? authorization,
        string? requestId,
        string? apiKey)
    {
        var request = new HttpRequestMessage(method, uri);
        if (authorization != null)
        {
            AddAuthorization(request, authorization);
        }

        if (requestId != null)
        {
            AddRequestIdHeader(request, requestId);
        }

        if (apiKey != null)
        {
            AddApiKeyHeader(request, apiKey);
        }

        return _httpClient.SendAsync(request);
    }

    private Task<HttpResponseMessage> SendWithContentAsync<T>(
        HttpMethod method,
        string uri,
        T content,
        string? authorization,
        string? requestId,
        string? apiKey)
    {
        var httpContent = GetHttpContent(content);
        var request = new HttpRequestMessage(method, uri)
        {
            Content = httpContent
        };

        if (authorization != null)
        {
            AddAuthorization(request, authorization);
        }

        if (requestId != null)
        {
            AddRequestIdHeader(request, requestId);
        }

        if (apiKey != null)
        {
            AddApiKeyHeader(request, apiKey);
        }

        return _httpClient.SendAsync(request);
    }

    private static void AddRequestIdHeader(HttpRequestMessage request, string requestId)
    {
        request.Headers.Add(HttpHeaderKeys.RequestId, requestId);
    }

    private static void AddApiKeyHeader(HttpRequestMessage request, string apikey)
    {
        request.Headers.Add(HttpHeaderKeys.ApiKey, apikey);
    }

    private static void AddAuthorization(HttpRequestMessage request, string authorization)
    {
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorization);
    }
}