using System.Diagnostics;
using System.Net;
using System.Text.Json;
using FinanceManager.File.Models;
using FinanceManager.File.Models.External;
using FinanceManager.Web;
using FinanceManager.Web.Models;
using Microsoft.Extensions.Options;
using RestSharp;
using Polly;
using Polly.CircuitBreaker;

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

public class FinanceManagerRestClient : IFinanceManagerRestClient
{
    private const int ExeptionsAllowedBeforeBreaking = 3;
    private static readonly TimeSpan BlockTimeout = TimeSpan.FromSeconds(30);
    private readonly AsyncCircuitBreakerPolicy _restClientBreaker;

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
        void OnBreak(Exception exception, TimeSpan timeSpan, Context context)
        {
            _logger.LogWarning(
                "Circuit breaker for sending via REST client was break with error. Message: {ErrorMessage}",
                exception.Message);
        }

        void OnReset(Context _) => _logger.LogInformation("Circuit breaker for sending via REST client");

        _restClientBreaker = Policy
            .Handle<WebException>()
            .CircuitBreakerAsync(ExeptionsAllowedBeforeBreaking, BlockTimeout, OnBreak, OnReset);

    }

    public Task<Account[]> GetAccountsByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Account[]>(
            _settings.AccountServiceBaseUrl,
            $"api/v1/accounts/internal?userId={userId}",
            transactionId);
    }

    public Task<Account[]> CreateAccountsByTransactionIdAsync(
        CreateAccountModel[] models,
        string transactionId,
        long userId)
    {
        return InternalExecuteAsync<Account[]>(
            _settings.AccountServiceBaseUrl,
            $"api/v1/accounts/{userId}/bulk",
            transactionId,
            models,
            Method.Post);
    }

    public Task RejectAccountsByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.AccountServiceBaseUrl,
            "api/v1/accounts",
            transactionId,
            Method.Delete);
    }

    public Task<Currency[]> GetCurrenciesByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Currency[]>(
            _settings.AccountServiceBaseUrl,
            $"api/v1/currencies/internal?userId={userId}",
            transactionId);
    }

    public Task<Currency[]> CreateCurrenciesByTransactionIdAsync(
        CreateCurrencyModel[] models,
        string transactionId,
        long userId)
    {
        return InternalExecuteAsync<Currency[]>(
            _settings.AccountServiceBaseUrl,
            $"api/v1/currencies/{userId}/bulk",
            transactionId,
            models,
            Method.Post);
    }

    public Task RejectCurrenciesByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.AccountServiceBaseUrl,
            "api/v1/currencies",
            requestId: transactionId,
            method: Method.Delete);
    }

    public Task<Category[]> GetCategoriesByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Category[]>(
            _settings.AccountServiceBaseUrl,
            $"api/v1/categories/internal?userId={userId}",
            transactionId);
    }

    public Task<Category[]> CreateCategoriesByTransactionIdAsync(
        CreateCategoryModel[] models,
        string transactionId,
        long userId)
    {
        return InternalExecuteAsync<Category[]>(
            _settings.AccountServiceBaseUrl,
            $"api/v1/categories/{userId}/bulk",
            transactionId,
            models,
            Method.Post);
    }

    public Task RejectCategoriesByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.AccountServiceBaseUrl,
            "api/v1/categories",
            transactionId,
            Method.Delete);
    }

    public Task<Deposit[]> GetDepositsByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Deposit[]>(
            _settings.DepositServiceBaseUrl,
            $"api/v1/deposits/internal?userId={userId}",
            transactionId);
    }

    public Task RejectDepositsByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.DepositServiceBaseUrl,
            "api/v1/deposits",
            transactionId,
            Method.Delete);
    }

    public Task<Expense[]> GetExpensesByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Expense[]>(
            _settings.ExpenseServiceBaseUrl,
            $"api/v1/expenses/internal?userId={userId}",
            transactionId);
    }

    public Task RejectExpensesByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.ExpenseServiceBaseUrl,
            "api/v1/expenses",
            transactionId,
            Method.Delete);
    }

    public Task<Transfer[]> GetTransfersByTransactionIdAsync(long? userId, string? transactionId)
    {
        return InternalExecuteAsync<Transfer[]>(
            _settings.TransferServiceBaseUrl,
            $"api/v1/transfers/internal?userId={userId}",
            transactionId);
    }

    public Task RejectTransfersByTransactionIdAsync(string transactionId)
    {
        return InternalExecuteAsync(
            _settings.TransferServiceBaseUrl,
            "api/v1/transfers",
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
      var restResponse = await _restClientBreaker.ExecuteAsync(async () => await restClient.ExecuteAsync(restRequest));
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