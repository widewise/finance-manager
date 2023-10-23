using FinanceManager.File.Models;
using FinanceManager.File.Models.External;

namespace FinanceManager.File.Services;

public interface IImportCurrenciesService
{
    Task<Dictionary<string, Guid>> ImportAsync(ImportSession session, FileContentItem[] fileItems);
}

public class ImportCurrenciesService : IImportCurrenciesService
{
    private readonly ILogger<ImportCurrenciesService> _logger;
    private readonly IFinanceManagerRestClient _restClient;

    public ImportCurrenciesService(
        ILogger<ImportCurrenciesService> logger,
        IFinanceManagerRestClient restClient)
    {
        _logger = logger;
        _restClient = restClient;
    }

    public async Task<Dictionary<string, Guid>> ImportAsync(
        ImportSession session,
        FileContentItem[] fileItems)
    {
        var items = fileItems
            .Where(x => !string.IsNullOrEmpty(x.ToCurrencyName))
            .Select(x => x.ToCurrencyName)
            .Union(fileItems
                .Where(x => !string.IsNullOrEmpty(x.FromCurrencyName))
                .Select(x => x.FromCurrencyName))
            .Distinct()
            .ToArray();
        var externalItems = await _restClient.GetCurrenciesByTransactionIdAsync(null, session.RequestId);
        var result = externalItems.ToDictionary(x => x.Name, x => x.Id);

        var modelsToAdd = items
            .Where(x => !result.ContainsKey(x))
            .Select(x => new CreateCurrencyModel
            {
                Name = x,
                ShortName = x
            })
            .ToArray();
        if (!modelsToAdd.Any())
        {
            return result;
        }

        var createdItems = await _restClient.CreateCurrenciesByTransactionIdAsync(
            modelsToAdd,
            session.RequestId,
            session.UserId);
        foreach (var createdItem in createdItems)
        {
            if (!result.TryAdd(createdItem.Name, createdItem.Id))
            {
                _logger.LogWarning("Currency with name {Name} is not created", createdItem.Name);
            }
        }

        return result;
    }
}