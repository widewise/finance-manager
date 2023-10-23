namespace FinanceManager.File.Services;

public interface ICache<TModel> where TModel : class
{
    TModel? Get(string key);
    Task<TModel?> GetAsync(string key);
    bool Set(string key, TModel value, TimeSpan? ttl = null);
    Task<bool> SetAsync(string key, TModel value, TimeSpan? ttl = null);
    bool Delete(string key);
    Task<bool> DeleteAsync(string key);
    Task<bool> ClearEverythingAsync();
}