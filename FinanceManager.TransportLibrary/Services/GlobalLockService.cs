using System.Collections.Concurrent;
using RedLockNet;

namespace FinanceManager.TransportLibrary.Services;

  public class GlobalLockService : IDistributedLockFactory
  {
    private static readonly ConcurrentDictionary<string, RefCounted<SemaphoreSlim>> GlobalLockers = new();

    private sealed class RefCounted<T>
    {
      public RefCounted(T value)
      {
        RefCount = 1;
        Value = value;
      }

      public int RefCount { get; set; }
      public T Value { get; private set; }
    }

    private static RefCounted<SemaphoreSlim> GetOrCreate(string key)
    {
      return GlobalLockers.AddOrUpdate(key
        , new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1))
        , (_, v) => { v.RefCount++; return v; });
    }

    private static RefCounted<SemaphoreSlim>? CreateOrNull(string key)
    {
      var item = new RefCounted<SemaphoreSlim>(new SemaphoreSlim(1, 1));
      lock (GlobalLockers)
      {
        if (GlobalLockers.TryAdd(key, item))
        {
          return item;
        }

        return null;
      }
    }

    public async Task<IDisposable> LockAsync(string key)
    {
      var refs = GetOrCreate(key);
      await refs.Value.WaitAsync().ConfigureAwait(false);
      return new Releaser(key, GlobalLockers);
    }

    public IRedLock CreateLock(string resource, TimeSpan expiryTime)
    {
      var refs = CreateOrNull(resource);
      refs?.Value.Wait(expiryTime);
      return new Releaser(resource, GlobalLockers);
    }
    public IRedLock CreateLock(string resource, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime, CancellationToken? cancellationToken = null)
    {
      var refs = GetOrCreate(resource);
      refs.Value.Wait(waitTime);
      return new Releaser(resource, GlobalLockers);
    }

    public async Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime)
    {
      var refs = CreateOrNull(resource);
      if (refs != null)
      {
        await refs.Value.WaitAsync(expiryTime).ConfigureAwait(false);        
      }
      return new Releaser(resource, GlobalLockers);
    }

    public async Task<IRedLock> CreateLockAsync(string resource, TimeSpan expiryTime, TimeSpan waitTime, TimeSpan retryTime, CancellationToken? cancellationToken = null)
    {
      var refs = GetOrCreate(resource);
      await refs.Value.WaitAsync(waitTime).ConfigureAwait(false);
      return new Releaser(resource, GlobalLockers);
    }

    public Task ReleaseLock(string key)
    {
      var refs = GetOrCreate(key);
      refs.Value.Release();
      return Task.CompletedTask;
    }

    private sealed class Releaser : IRedLock
    {
      private readonly ConcurrentDictionary<string, RefCounted<SemaphoreSlim>> _lockers;
      public Releaser(string key, ConcurrentDictionary<string, RefCounted<SemaphoreSlim>> lockers)
      {
        Key = key;
        _lockers = lockers ?? throw new ArgumentNullException(nameof(lockers));
        LockId = Guid.NewGuid().ToString("N");
      }

      public string Key { get; }

      public string Resource => Key;

      public string LockId { get; }
      public bool IsAcquired { get; private set; } = true;

      public RedLockStatus Status => IsAcquired ? RedLockStatus.Acquired : RedLockStatus.Conflicted;

      public RedLockInstanceSummary InstanceSummary => default(RedLockInstanceSummary);

      public int ExtendCount => 0;

      public void Dispose()
      {
        if (_lockers.TryGetValue(Key, out var item))
        {
          --item.RefCount;
          if (item.RefCount <= 0)
            _lockers.TryRemove(Key, out _);
          item.Value.Release();
        }

        IsAcquired = false;
      }

      public ValueTask DisposeAsync()
      {
        if (_lockers.TryGetValue(Key, out var item))
        {
          --item.RefCount;
          if (item.RefCount <= 0)
            _lockers.TryRemove(Key, out _);
          item.Value.Release();
        }

        IsAcquired = false;
        return ValueTask.CompletedTask;
      }
    }
  }