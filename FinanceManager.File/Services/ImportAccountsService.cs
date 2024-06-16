using FinanceManager.File.Models;
using FinanceManager.File.Models.External;

namespace FinanceManager.File.Services;

public interface IImportAccountsService
{
    Task<Dictionary<string, Guid>> ImportAsync(
        ImportSession session,
        string fileName,
        Dictionary<string, Guid> currencies,
        FileContentItem[] fileItems);
}

public class ImportAccountsService : IImportAccountsService
{
    private readonly ILogger<ImportAccountsService> _logger;
    private readonly IFinanceManagerRestClient _restClient;

    public ImportAccountsService(
        ILogger<ImportAccountsService> logger,
        IFinanceManagerRestClient restClient)
    {
        _logger = logger;
        _restClient = restClient;
    }

    public async Task<Dictionary<string, Guid>> ImportAsync(
        ImportSession session,
        string fileName,
        Dictionary<string, Guid> currencies,
        FileContentItem[] fileItems)
    {
        var items = fileItems
            .Where(x => !string.IsNullOrEmpty(x.ToAccountName) && !string.IsNullOrEmpty(x.ToCurrencyName))
            .Select(x => new
            {
                Account = x.ToAccountName,
                CurrencyId = currencies[x.ToCurrencyName!]
            })
            .Union(fileItems
                .Where(x => !string.IsNullOrEmpty(x.FromAccountName) && !string.IsNullOrEmpty(x.FromCurrencyName))
                .Select(x => new
            {
                Account = x.FromAccountName,
                CurrencyId = currencies[x.FromCurrencyName!]
            }))
            .ToDictionary(x => x.Account, x => x.CurrencyId);
        var externalItems = await _restClient.GetAccountsByTransactionIdAsync(null, session.RequestId);
        var result = externalItems.ToDictionary(x => x.Name, x => x.Id);

        var modelsToAdd = items
            .Where(x => x.Key != null && !result.ContainsKey(x.Key))
            .Select(x => new CreateAccountModel
            {
                Name = x.Key!,
                CurrencyId = x.Value,
                Description = $"Imported from file {fileName}"
            })
            .ToArray();
        if (!modelsToAdd.Any())
        {
            return result;
        }

        var createdItems = await _restClient.CreateAccountsByTransactionIdAsync(
            modelsToAdd,
            session.RequestId,
            session.UserId);
        foreach (var createdItem in createdItems)
        {
            if (!result.TryAdd(createdItem.Name, createdItem.Id))
            {
                _logger.LogWarning("Account with name {Name} is not created", createdItem.Name);
            }
        }

        return result;
    }
}