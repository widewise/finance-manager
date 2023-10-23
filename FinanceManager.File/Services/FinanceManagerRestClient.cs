using System.Diagnostics;
using System.Net;
using System.Text.Json;
using FinanceManager.File.Models;
using FinanceManager.File.Models.External;
using FinanceManager.TransportLibrary;
using FinanceManager.TransportLibrary.Models;
using Microsoft.Extensions.Options;
using RestSharp;

namespace FinanceManager.File.Services;

public interface IFinanceManagerRestClient
{
    Task<Account[]> GetAccountsByTransactionIdAsync(long? userId, string? transactionId);
    Task<Account[]> CreateAccountsByTransactionIdAsync(
        CreateAccountModel[] models,
        string transactionId,
        long userId);
    Task RejectAccountsByTransactionIdAsync(string transactionId);
    Task<Currency[]> GetCurrenciesByTransactionIdAsync(long? userId, string? transactionId);
    Task<Currency[]> CreateCurrenciesByTransactionIdAsync(
        CreateCurrencyModel[] models,
        string transactionId,
        long userId);
    Task RejectCurrenciesByTransactionIdAsync(string transactionId);
    Task<Category[]> GetCategoriesByTransactionIdAsync(long? userId, string? transactionId);
    Task<Category[]> CreateCategoriesByTransactionIdAsync(
        CreateCategoryModel[] models,
        string transactionId,
        long userId);
    Task RejectCategoriesByTransactionIdAsync(string transactionId);
    Task<Deposit[]> GetDepositsByTransactionIdAsync(long? userId, string? transactionId);
    Task RejectDepositsByTransactionIdAsync(string transactionId);
    Task<Expense[]> GetExpensesByTransactionIdAsync(long? userId, string? transactionId);
    Task RejectExpensesByTransactionIdAsync(string transactionId);
    Task<Transfer[]> GetTransfersByTransactionIdAsync(long? userId, string? transactionId);
    Task RejectTransfersByTransactionIdAsync(string transactionId);
}

//TODO: add circuit breaker
public class FinanceManagerRestClient : IFinanceManagerRestClient
{
    private readonly ILogger<FinanceManagerRestClient> _logger;
    private readonly FinanceManagerSettings _settings;
    private readonly ApiKeySettings _apiKeySettings;

    public FinanceManagerRestClient(
        ILogger<FinanceManagerRestClient> logger,
        IOptions<FinanceManagerSettings> settings,
        IOptions<ApiKeySettings> apiKeySettings)
    {
        _logger = logger;
        _settings = settings.Value;
        _apiKeySettings = apiKeySettings.Value;
    }

    public Task<Account[]> GetAccountsByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Account[]>(
            _settings.AccountServiceBaseUrl,
            $"api/account/internal?userId={userId}",
            transactionId);
    }

    public Task<Account[]> CreateAccountsByTransactionIdAsync(
        CreateAccountModel[] models,
        string transactionId,
        long userId)
    {
        return InternalExecuteAsync<Account[]>(
            _settings.AccountServiceBaseUrl,
            $"api/account/{userId}/bulk",
            transactionId,
            models,
            Method.Post);
    }

    public Task RejectAccountsByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.AccountServiceBaseUrl,
            "api/account",
            transactionId,
            Method.Delete);
    }

    public Task<Currency[]> GetCurrenciesByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Currency[]>(
            _settings.AccountServiceBaseUrl,
            $"api/currency/internal?userId={userId}",
            transactionId);
    }

    public Task<Currency[]> CreateCurrenciesByTransactionIdAsync(
        CreateCurrencyModel[] models,
        string transactionId,
        long userId)
    {
        return InternalExecuteAsync<Currency[]>(
            _settings.AccountServiceBaseUrl,
            $"api/currency/{userId}/bulk",
            transactionId,
            models,
            Method.Post);
    }

    public Task RejectCurrenciesByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.AccountServiceBaseUrl,
            "api/currency",
            requestId: transactionId,
            method: Method.Delete);
    }

    public Task<Category[]> GetCategoriesByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Category[]>(
            _settings.AccountServiceBaseUrl,
            $"api/category/internal?userId={userId}",
            transactionId);
    }

    public Task<Category[]> CreateCategoriesByTransactionIdAsync(
        CreateCategoryModel[] models,
        string transactionId,
        long userId)
    {
        return InternalExecuteAsync<Category[]>(
            _settings.AccountServiceBaseUrl,
            $"api/category/{userId}/bulk",
            transactionId,
            models,
            Method.Post);
    }

    public Task RejectCategoriesByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.AccountServiceBaseUrl,
            "api/category",
            transactionId,
            Method.Delete);
    }

    public Task<Deposit[]> GetDepositsByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Deposit[]>(
            _settings.DepositServiceBaseUrl,
            $"api/deposit/internal?userId={userId}",
            transactionId);
    }

    public Task RejectDepositsByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.DepositServiceBaseUrl,
            "api/deposit",
            transactionId,
            Method.Delete);
    }

    public Task<Expense[]> GetExpensesByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Expense[]>(
            _settings.ExpenseServiceBaseUrl,
            $"api/expense/internal?userId={userId}",
            transactionId);
    }

    public Task RejectExpensesByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.ExpenseServiceBaseUrl,
            "api/expense",
            transactionId,
            Method.Delete);
    }

    public Task<Transfer[]> GetTransfersByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Transfer[]>(
            _settings.TransferServiceBaseUrl,
            $"api/transfer/internal?userId={userId}",
            transactionId);
    }

    public Task RejectTransfersByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.TransferServiceBaseUrl,
            "api/transfer",
            transactionId,
            Method.Delete);
    }
    
    private async Task<TResponse> InternalExecuteAsync<TResponse>(
        string baseUrl,
        string requestPath,
        string? requestId = null,
        object? bodyObject = null,
        Method method = Method.Get)
    {
        TResponse? result = default;
        await InternalExecuteAsync(
            baseUrl,
            requestPath,
            requestId,
            method,
            bodyObject,
            response =>
            {
                result = response.StatusCode == HttpStatusCode.NoContent
                    ? default
                    : JsonSerializer.Deserialize<TResponse>(response.Content!, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            });

        if (result == null)
        {
            throw new WebException("No content");
        }

        return result;
    }

    private async Task InternalExecuteAsync(
      string baseUrl,
      string requestPath,
      string? requestId = null,
      Method method = Method.Get,
      object? bodyObject = null,
      Action<RestResponse>? onSuccessCallback = null)
    {
      var requestRow = new Uri(new Uri(baseUrl), requestPath).ToString();
      _logger.LogDebug(
        "Starting request {RequestRow} with request id {RequestId}",
        requestRow,
        requestId);
      var restClient = new RestClient(baseUrl);
      restClient.AddDefaultHeader(HttpHeaderKeys.ApiKey, _apiKeySettings.ApiKey);
      if (!string.IsNullOrEmpty(requestId))
      {
          restClient.AddDefaultHeader(HttpHeaderKeys.RequestId, requestId);
      }
      var restRequest = new RestRequest(requestPath, method);
      if (bodyObject != null)
      {
          restRequest.AddJsonBody(bodyObject);
      }
      Stopwatch stopwatch = Stopwatch.StartNew();
      var restResponse = await restClient.ExecuteAsync(restRequest);
      stopwatch.Stop();
      if (restResponse.IsSuccessful)
      {
        _logger.LogDebug("Request {RequestRow} successfully processed!", requestRow);
        if (onSuccessCallback != null)
        {
            onSuccessCallback(restResponse);
        }
      }
      else
      {
        if (restResponse.StatusCode == HttpStatusCode.GatewayTimeout)
        {
            var requestBody = restRequest.Parameters
                           .FirstOrDefault(p => p.Type == ParameterType.RequestBody)?.Value?.ToString()
                       ?? string.Empty;
            var requestParameters = string.Join("\t\n", restRequest.Parameters.Select((Func<Parameter, string>)(
                x => $"{x.Name}({x.Type}) = {x.Value}")));
            var message = $"Request took {stopwatch.ElapsedMilliseconds}ms to complete. \nRequest Resource: {restRequest.Resource}\nRequest Body: {requestBody}\nRequest Parameters: {requestParameters}";
          _logger.LogError(message);
          throw restResponse.ErrorException ??
                (Exception) new WebException($"{restResponse.StatusCode}: {restResponse.Content}\n Request Details: \n {message}");
        }
        throw restResponse.ErrorException ??
              (Exception) new WebException($"{(object)restResponse.StatusCode}: {(object)restResponse.Content!}");
      }
    }
}