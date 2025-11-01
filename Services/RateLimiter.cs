using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonCardManager.Services
{
    public interface IRateLimiter
    {
        Task WaitForSlotAsync(string key, CancellationToken cancellationToken = default);
        void Reset(string key);
    }

    public class RateLimiter : IRateLimiter
    {
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();
        private readonly int _maxConcurrentRequests;
        private readonly TimeSpan _timeWindow;
        private readonly ILogger? _logger;

        public RateLimiter(int maxConcurrentRequests = 5, TimeSpan? timeWindow = null, ILogger? logger = null)
        {
            _maxConcurrentRequests = maxConcurrentRequests;
            _timeWindow = timeWindow ?? TimeSpan.FromSeconds(1);
            _logger = logger;
        }

        public async Task WaitForSlotAsync(string key, CancellationToken cancellationToken = default)
        {
            var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(_maxConcurrentRequests, _maxConcurrentRequests));

            await semaphore.WaitAsync(cancellationToken);

            // Schedule release after time window
            _ = Task.Run(async () =>
            {
                await Task.Delay(_timeWindow, cancellationToken);
                semaphore.Release();
            }, cancellationToken);

            _logger?.LogDebug($"Rate limiter: acquired slot for key: {key}");
        }

        public void Reset(string key)
        {
            if (_semaphores.TryRemove(key, out var semaphore))
            {
                try
                {
                    semaphore.Dispose();
                }
                catch { }
                _logger?.LogDebug($"Rate limiter: reset for key: {key}");
            }
        }
    }
}

