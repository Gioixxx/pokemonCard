using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key) where T : class;
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
        void Remove(string key);
        void Clear();
        int Count { get; }
    }

    public class CacheService : ICacheService
    {
        private readonly ConcurrentDictionary<string, CacheItem> _cache = new();
        private readonly ILogger _logger;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(24);

        public CacheService(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public int Count => _cache.Count;

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            if (string.IsNullOrWhiteSpace(key))
                return Task.FromResult<T?>(null);

            if (_cache.TryGetValue(key, out var item))
            {
                if (item.IsExpired)
                {
                    _cache.TryRemove(key, out _);
                    _logger.LogDebug($"Cache expired for key: {key}");
                    return Task.FromResult<T?>(null);
                }

                _logger.LogDebug($"Cache hit for key: {key}");
                return Task.FromResult(item.Value as T);
            }

            _logger.LogDebug($"Cache miss for key: {key}");
            return Task.FromResult<T?>(null);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (string.IsNullOrWhiteSpace(key) || value == null)
                return Task.CompletedTask;

            var expiry = expiration ?? _defaultExpiration;
            var cacheItem = new CacheItem(value, DateTime.UtcNow.Add(expiry));
            _cache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
            _logger.LogDebug($"Cached item with key: {key}, expires in: {expiry}");
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            _cache.TryRemove(key, out _);
            _logger.LogDebug($"Removed cache item with key: {key}");
        }

        public void Clear()
        {
            var count = _cache.Count;
            _cache.Clear();
            _logger.LogInformation($"Cleared cache ({count} items removed)");
        }

        private class CacheItem
        {
            public object Value { get; }
            public DateTime ExpiresAt { get; }

            public bool IsExpired => DateTime.UtcNow > ExpiresAt;

            public CacheItem(object value, DateTime expiresAt)
            {
                Value = value ?? throw new ArgumentNullException(nameof(value));
                ExpiresAt = expiresAt;
            }
        }
    }
}

