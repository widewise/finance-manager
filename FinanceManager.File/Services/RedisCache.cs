using System.Text.Json;
using StackExchange.Redis;

namespace FinanceManager.File.Services;

public class RedisCache<TModel> : RedisCache, ICache<TModel>
    where TModel : class
{
    public RedisCache(IConfiguration configuration) :
        base(configuration.GetConnectionString("redis") ?? string.Empty)
    {

    }

    public TModel? Get(string key)
    {
        var cache = GetInstance();
        RedisValue value = cache.StringGet(BaseKey + key);
        if (value.HasValue)
        {
            return JsonSerializer.Deserialize<TModel>(value.ToString());
        }

        return default;
    }


    public async Task<TModel?> GetAsync(string key)
    {
        var cache = GetInstance();
        RedisValue value = await cache.StringGetAsync(BaseKey + key);
        if (value.HasValue)
        {
            return JsonSerializer.Deserialize<TModel>(value.ToString());
        }

        return default;
    }


    public bool Set(string key, TModel value, TimeSpan? ttl = null)
    {
        var cache = GetInstance();
        return cache.StringSet(
            BaseKey + key,
            JsonSerializer.Serialize(value),
            ttl);
    }


    public async Task<bool> SetAsync(string key, TModel value, TimeSpan? ttl = null)
    {
        var cache = GetInstance();
        return await cache.StringSetAsync(
            BaseKey + key,
            JsonSerializer.Serialize(value),
            ttl);
    }



    public async Task<bool> DeleteAsync(string key)
    {
        var cache = GetInstance();
        return await cache.KeyDeleteAsync(BaseKey + key);
    }


    public bool Delete(string key)
    {
        var cache = GetInstance();
        return cache.KeyDelete(BaseKey + key);
    }

}

public abstract class RedisCache
{
    protected const string BaseKey = "RetailRosiService:";
    private readonly string _redisConnectionString;
    protected readonly Lazy<ConnectionMultiplexer> Redis;

    protected RedisCache(string redisConnectionString)
    {
        _redisConnectionString = redisConnectionString;
        Redis = new Lazy<ConnectionMultiplexer>(
            () => ConnectionMultiplexer.Connect(_redisConnectionString));
    }

    protected IDatabase GetInstance()
    {
        IDatabase cache = Redis.Value.GetDatabase();
        return cache;
    }

    public async Task<bool> ClearEverythingAsync()
    {
        var hostAndPort = _redisConnectionString.Split(',')[0];

        if (!Redis.IsValueCreated)
        {
            return false;
        }

        var server = Redis.Value.GetServer(hostAndPort);
        var keys = server.Keys(pattern: BaseKey + "*");

        IDatabase cache = Redis.Value.GetDatabase();
        foreach (var key in keys)
        {
            await cache.KeyDeleteAsync(key);
        }

        return true;
    }
}