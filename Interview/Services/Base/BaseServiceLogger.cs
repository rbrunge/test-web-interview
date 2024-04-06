using Microsoft.Extensions.Caching.Memory;

namespace Interview.Services.Base;

public abstract class BaseServiceLogger
{
    private const string _source = "Service";
    private const string _messageFound = "From {0}";
    private const string _messageNotFound = "Not Found";
    private const string _messageFoundCached = "From Cache";
    private const string _messageException = "Exception";

    private readonly ILogger _logger;
    private readonly IMemoryCache _memoryCache;

    protected BaseServiceLogger(ILogger logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
    }

    protected void LogFoundOrNotFound(bool found, string message, string source = _source)
    { if (found) LogFound(message, source); else LogNotFound(message); }

    protected void LogFound(string message, string source = _source)
        => _logger.LogInformation("{message}", string.Format(message, string.Format(_messageFound, source)));

    protected void LogNotFound(string message)
        => _logger.LogWarning("{message}", string.Format(message, _messageNotFound));

    protected void LogFoundCached(string message)
        => _logger.LogInformation("{message}", string.Format(message, _messageFoundCached));

    protected void LogException(Exception exception, string message)
        => _logger.LogError(exception, "{message}", string.Format(message, _messageException));

    protected async Task<dynamic> CacheReadThrough(string cacheKey, Func<Task<dynamic>> fetch, string logMessage = null, string logSource = null)
    {
        if (_memoryCache.TryGetValue(cacheKey, out dynamic result))
        {
            LogFoundCached(cacheKey);
            return result;
        }

        result = await fetch();

        if (result is not null)
            CacheExtensions.Set(_memoryCache, cacheKey, result, DateTime.Now.AddSeconds(10));

        LogFoundOrNotFound(result != null, logMessage, logSource);
        return result;
    }

    protected async Task<dynamic> CacheReadThroughAlternative(string cacheKey, Func<Task<dynamic>> fetch, string logMessage = null, string logSource = null)
    {
        return await _memoryCache.GetOrCreateAsync<dynamic>(cacheKey, async entry =>
        {
            dynamic result = await fetch();
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
            LogFoundOrNotFound(result != null, logMessage, logSource);
            return result;
        });
    }

}