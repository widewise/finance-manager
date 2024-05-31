using System.Diagnostics.CodeAnalysis;
using FinanceManager.Testing.Clients.Exceptions;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace FinanceManager.Testing.Clients;

public interface IRestServiceClient
{
    Task<TResponse> GetAsync<TResponse>(
        Uri uri,
        string? requestId = null,
        string? apiKey = null);

    Task<TResponse> PostAsync<TResponse, TRequest>(
        Uri uri,
        TRequest request,
        string? requestId = null,
        string? apiKey = null);

    Task<TResponse> PutAsync<TResponse, TRequest>(
        Uri uri,
        TRequest request,
        string? requestId = null,
        string? apiKey = null);

    Task DeleteAsync(
        Uri uri,
        string? requestId = null,
        string? apiKey = null);
}

[ExcludeFromCodeCoverage]
internal class RestServiceClient : IRestServiceClient
{
    private readonly IHttpClient _httpClient;
    private readonly ITestOutputHelper _output;
    private readonly string? _authorization;

    public RestServiceClient(
        IHttpClient httpClient,
        ITestOutputHelper output,
        string? authorization)
    {
        _httpClient = httpClient;
        _output = output;
        _authorization = authorization;
    }

    public async Task<TResponse> GetAsync<TResponse>(
        Uri uri,
        string? requestId = null,
        string? apiKey = null)
    {
        var resolverRequestId = ResolverRequestId(requestId);
        var httpResponse = await PerformRequestAsync(uri, nameof(GetAsync), resolverRequestId,
            () => _httpClient.GetAsync(uri.AbsoluteUri, _authorization, resolverRequestId, apiKey));

        return await DeserializeAsync<TResponse>(httpResponse.Content);
    }

    public async Task<TResponse> PostAsync<TResponse, TRequest>(
        Uri uri,
        TRequest request,
        string? requestId = null,
        string? apiKey = null)
    {
        var resolverRequestId = ResolverRequestId(requestId);
        var httpResponse = await PerformRequestAsync(uri, nameof(PostAsync), resolverRequestId,
            () => _httpClient.PostAsync(uri.AbsoluteUri, request, _authorization, resolverRequestId, apiKey));

        return await DeserializeAsync<TResponse>(httpResponse.Content);
    }

    public async Task<TResponse> PutAsync<TResponse, TRequest>(
        Uri uri,
        TRequest request,
        string? requestId = null,
        string? apiKey = null)
    {
        var resolverRequestId = ResolverRequestId(requestId);
        var httpResponse = await PerformRequestAsync(uri, nameof(PutAsync), resolverRequestId,
            () => _httpClient.PutAsync(uri.AbsoluteUri, request, _authorization, resolverRequestId, apiKey));

        return await DeserializeAsync<TResponse>(httpResponse.Content);
    }

    public async Task DeleteAsync(
        Uri uri,
        string? requestId = null,
        string? apiKey = null)
    {
        var resolverRequestId = ResolverRequestId(requestId);
        await PerformRequestAsync(uri, nameof(DeleteAsync), resolverRequestId,
            () => _httpClient.DeleteAsync(uri.AbsoluteUri, _authorization, resolverRequestId, apiKey));
    }

    private string ResolverRequestId(string? requestId)
    {
        if (!string.IsNullOrWhiteSpace(requestId))
        {
            return requestId;
        }

        return Guid.NewGuid().ToString();
    }

    private async Task<HttpResponseMessage> PerformRequestAsync(
        Uri uri,
        string restMethod,
        string requestId,
        Func<Task<HttpResponseMessage>> sendRequestAction)
    {
        HttpResponseMessage httpResponse;
        try
        {
            httpResponse = await sendRequestAction();
        }
        catch (Exception exception)
        {
            _output.WriteLine(
                "{Method}: An exception has occurred while attempting to send request {Uri} with request id {rId}",
                restMethod,
                uri,
                requestId);
            throw new RestServiceClientException(restMethod, uri, requestId, exception);
        }

        if (!httpResponse.IsSuccessStatusCode)
        {
            var reason = await httpResponse.Content.ReadAsStringAsync();
            _output.WriteLine($"{restMethod}: The status code of sending to {uri} HTTP response is {httpResponse.StatusCode}. Request id: {requestId}. Reason:{reason}. Response: {httpResponse}");
            throw new RestServiceClientException(restMethod, uri, requestId, reason, httpResponse);
        }

        return httpResponse;
    }

    private async Task<TResponse> DeserializeAsync<TResponse>(HttpContent httpContent)
    {
        var serializer = new JsonSerializer();
        Stream? stream = null;
        try
        {
            stream = await httpContent.ReadAsStreamAsync();
            using var streamReader = new StreamReader(stream);
            using var reader = new JsonTextReader(streamReader);
            var customizationModel = serializer.Deserialize<TResponse>(reader);
            return customizationModel;
        }
        finally
        {
            if (stream != null)
            {
                await stream.DisposeAsync();
            }
        }
    }
}