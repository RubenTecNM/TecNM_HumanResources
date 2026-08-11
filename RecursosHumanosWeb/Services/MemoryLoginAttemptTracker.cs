using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace RecursosHumanosWeb.Services;

public sealed class LoginAttemptOptions
{
    public const string SectionName = "Security:LoginAttempts";

    public int MaxAttempts { get; set; } = 5;
    public int ResetWindowMinutes { get; set; } = 15;
    public int LockoutMinutes { get; set; } = 15;
}

public sealed class MemoryLoginAttemptTracker : ILoginAttemptTracker
{
    private const string CounterKeyPrefix = "login:attempts:";
    private const string LockKeyPrefix = "login:lock:";

    private readonly IMemoryCache _cache;
    private readonly LoginAttemptOptions _options;

    public MemoryLoginAttemptTracker(IMemoryCache cache, IOptions<LoginAttemptOptions> options)
    {
        _cache = cache;
        _options = options.Value;
    }

    public bool IsLockedOut(string key)
    {
        var normalized = Normalize(key);
        if (string.IsNullOrEmpty(normalized)) return false;
        return _cache.TryGetValue(LockKeyPrefix + normalized, out _);
    }

    public void RegisterFailure(string key)
    {
        var normalized = Normalize(key);
        if (string.IsNullOrEmpty(normalized)) return;

        var counterKey = CounterKeyPrefix + normalized;
        var lockKey = LockKeyPrefix + normalized;

        int failures = _cache.TryGetValue(counterKey, out int current) ? current + 1 : 1;

        if (failures >= _options.MaxAttempts)
        {
            _cache.Remove(counterKey);
            _cache.Set(lockKey, true, TimeSpan.FromMinutes(_options.LockoutMinutes));
        }
        else
        {
            _cache.Set(counterKey, failures, TimeSpan.FromMinutes(_options.ResetWindowMinutes));
        }
    }

    public void Reset(string key)
    {
        var normalized = Normalize(key);
        if (string.IsNullOrEmpty(normalized)) return;
        _cache.Remove(CounterKeyPrefix + normalized);
        _cache.Remove(LockKeyPrefix + normalized);
    }

    private static string Normalize(string key) => key?.Trim().ToLowerInvariant() ?? string.Empty;
}
