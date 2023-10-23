using FinanceManager.File.Models;
using FinanceManager.File.Models.External;

namespace FinanceManager.File.Services;

public interface IImportCategoriesService
{
    Task<Dictionary<string, Guid>> ImportAsync(
        ImportSession session,
        string fileName,
        FileContentItem[] fileItems);
}

public class ImportCategoriesService : IImportCategoriesService
{
    private readonly ILogger<ImportCategoriesService> _logger;
    private readonly IFinanceManagerRestClient _restClient;

    public ImportCategoriesService(
        ILogger<ImportCategoriesService> logger,
        IFinanceManagerRestClient restClient)
    {
        _logger = logger;
        _restClient = restClient;
    }

    public async Task<Dictionary<string, Guid>> ImportAsync(
        ImportSession session,
        string fileName,
        FileContentItem[] fileItems)
    {
        var items = fileItems
            .Where(x => !string.IsNullOrEmpty(x.ToCategoryName) && string.IsNullOrEmpty(x.FromCategoryName))
            .Select(x => new
            {
                Name = x.ToCategoryName,
                Type = CategoryType.Deposit
            })
            .Union(fileItems
                .Where(x => !string.IsNullOrEmpty(x.FromCategoryName) && string.IsNullOrEmpty(x.ToCategoryName))
                .Select(x => new
                {
                    Name = x.FromCategoryName,
                    Type = CategoryType.Expense
                }))
            .Distinct()
            .ToArray();
        var externalItems = await _restClient.GetCategoriesByTransactionIdAsync(null, session.RequestId);
        var result = externalItems.ToDictionary(x => x.Name, x => x.Id);

        var modelsToAdd = items
            .Where(x => !result.ContainsKey(x.Name!))
            .Select(x => new CreateCategoryModel
            {
                Name = x.Name,
                Type = x.Type,
                Description = $"Imported from file {fileName}"
            })
            .ToArray();
        if (!modelsToAdd.Any())
        {
            return result;
        }

        var createdItems = await _restClient.CreateCategoriesByTransactionIdAsync(
            modelsToAdd,
            session.RequestId,
            session.UserId);
        foreach (var createdItem in createdItems)
        {
            if (!result.TryAdd(createdItem.Name, createdItem.Id))
            {
                _logger.LogWarning("Category with name {Name} is not created", createdItem.Name);
            }
        }

        return result;
    }
}