using Contentstack.Core;
using Contentstack.Core.Internals;
using Interview.Configuration;
using Interview.Services.Base;
using Interview.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Interview.Services;

public class ContentstackService : BaseServiceLogger, IContentstackService
{
    // TODO - 1. hardcoded values - introduce const for each
    // TODO - 2. private GetEntry - make static
    // TODO - 3. public GetEntry - make async
    // TODO - 4. MemoryCache - extract to base class (like BaseServiceLogger)
    // TODO - 5. outgoing calls - limit to 5 concurrent
    // TODO - 6. Contentstack - add IncludeReference to outgoing call
    // TODO - 7. Contentstack - introduce GetEntries method (with or without pagination?)
    // TODO - 8. Contentstack - support live preview

    private const int _timeoutSeconds = 60;
    private const int _cacheSeconds = 60;
    private const int _maxConcurrency = 5;
    private const string _logSource = "Stack";
    private const string _logMessageEntryPrefix = "Entry";
    private const string _cacheKeyPrefix = "Contentstack";
    private readonly string _locale;
    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(_timeoutSeconds);
    private readonly SemaphoreSlim _semaphore;
    private readonly ILogger<ContentstackService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly ContentstackClient _contentstackClient;

    public ContentstackService(
        ILogger<ContentstackService> logger,
        IMemoryCache memoryCache,
        IOptions<ContentstackConfiguration> contentstackConfiguration)
        : base(logger, memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        var apiKey = contentstackConfiguration.Value.ApiKey;
        var deliveryToken = contentstackConfiguration.Value.DeliveryToken;
        var environment = contentstackConfiguration.Value.Environment;

        // TODO - 1.hardcoded values - introduce const for each
        // A: not a const, using setting value instead.
        _locale = contentstackConfiguration.Value.Locale;
        _contentstackClient = new ContentstackClient(apiKey, deliveryToken, environment, null, ContentstackRegion.EU);
        _semaphore = new SemaphoreSlim(_maxConcurrency);
    }

    #region Original starting point

    public dynamic GetEntry(string contentTypeUid, string entryUid)
    {
        var logMessage = $"{_logMessageEntryPrefix} {{0}} {contentTypeUid} {entryUid}";

        var cacheKey = $"{_cacheKeyPrefix}:{contentTypeUid}:{entryUid}";

        if (_memoryCache.TryGetValue(cacheKey, out dynamic result))
        {
            LogFoundCached(logMessage);

            return result;
        }

        try
        {
            result = GetEntry(contentTypeUid, entryUid, _locale);
            CacheExtensions.Set(_memoryCache, cacheKey, result, DateTime.Now.AddSeconds(_cacheSeconds));

            LogFoundOrNotFound(result != null, logMessage, _logSource);
        }
        catch (Exception exception)
        {
            LogException(exception, logMessage);
        }

        return result;
    }

    private dynamic GetEntry(string contentTypeUid, string entryUid, string locale)
    {
        if (string.IsNullOrEmpty(contentTypeUid))
            return null;
        if (string.IsNullOrEmpty(entryUid))
            return null;

        var entry = _contentstackClient
            .ContentType(contentTypeUid)
            .Entry(entryUid)
            .SetLocale(locale)
            .includeEmbeddedItems();

        var timer = new Stopwatch();
        timer.Start();
        Task<dynamic> task = null;
        entry.Fetch<dynamic>().ContinueWith(t => task = t);
        while (task == null && timer.Elapsed < _timeout) { }

        if (task == null)
            throw new TimeoutException();
        if (task.IsFaulted)
            throw task.Exception;

        return task.Result;
    }

    #endregion Original starting point

    #region TODO - 2. private GetEntry - make static

    public dynamic GetEntryStatic(string contentTypeUid, string entryUid)
    {
        var logMessage = $"{_logMessageEntryPrefix} {{0}} {contentTypeUid} {entryUid}";

        var cacheKey = $"{_cacheKeyPrefix}:{contentTypeUid}:{entryUid}";

        if (_memoryCache.TryGetValue(cacheKey, out dynamic result))
        {
            LogFoundCached(logMessage);

            return result;
        }

        try
        {
            result = GetEntryStatic(contentTypeUid, entryUid, _locale, _contentstackClient);
            CacheExtensions.Set(_memoryCache, cacheKey, result, DateTime.Now.AddSeconds(_cacheSeconds));

            LogFoundOrNotFound(result != null, logMessage, _logSource);
        }
        catch (Exception exception)
        {
            LogException(exception, logMessage);
        }

        return result;
    }

    private static dynamic GetEntryStatic(string contentTypeUid, string entryUid, string locale, ContentstackClient contentstackClient)
    {
        if (string.IsNullOrEmpty(contentTypeUid))
            return null;
        if (string.IsNullOrEmpty(entryUid))
            return null;

        var entry = contentstackClient
            .ContentType(contentTypeUid)
            .Entry(entryUid)
            .SetLocale(locale)
            .includeEmbeddedItems();

        var timer = new Stopwatch();
        timer.Start();
        Task<dynamic> task = null;
        entry.Fetch<dynamic>().ContinueWith(t => task = t);
        while (task == null && timer.Elapsed < _timeout) { }

        if (task == null)
            throw new TimeoutException();
        if (task.IsFaulted)
            throw task.Exception;

        return task.Result;
    }

    #endregion TODO - 2. private GetEntry - make static

    #region TODO - 3. public GetEntry - make async

    public async Task<dynamic> GetEntryAsync(string contentTypeUid, string entryUid)
    {
        var logMessage = $"{_logMessageEntryPrefix} {{0}} {contentTypeUid} {entryUid}";

        var cacheKey = $"{_cacheKeyPrefix}:{contentTypeUid}:{entryUid}";

        if (_memoryCache.TryGetValue(cacheKey, out dynamic result))
        {
            LogFoundCached(logMessage);

            return result;
        }

        try
        {
            result = await GetEntryAsync(contentTypeUid, entryUid, _locale, _contentstackClient);
            CacheExtensions.Set(_memoryCache, cacheKey, result, DateTime.Now.AddSeconds(_cacheSeconds));

            LogFoundOrNotFound(result != null, logMessage, _logSource);
        }
        catch (Exception exception)
        {
            LogException(exception, logMessage);
        }

        return result;
    }

    private static async Task<dynamic> GetEntryAsync(string contentTypeUid, string entryUid, string locale, ContentstackClient contentstackClient)
    {
        if (string.IsNullOrEmpty(contentTypeUid))
            return null;
        if (string.IsNullOrEmpty(entryUid))
            return null;

        var entry = contentstackClient
            .ContentType(contentTypeUid)
            .Entry(entryUid)
            .SetLocale(locale)
            .includeEmbeddedItems();

        var task = entry.Fetch<dynamic>();
        var timeoutTask = Task.Delay(_timeout);
        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
            throw new TimeoutException();
        if (task.IsFaulted)
            throw task.Exception;

        return task.Result;
    }

    #endregion TODO - 3. public GetEntry - make async

    #region TODO - 4. MemoryCache - extract to base class (like BaseServiceLogger)

    public async Task<dynamic> GetEntryBaseCachingAsync(string contentTypeUid, string entryUid)
    {
        var logMessage = $"{_logMessageEntryPrefix} {{0}} {contentTypeUid} {entryUid}";
        var cacheKey = $"{_cacheKeyPrefix}:{contentTypeUid}:{entryUid}";
        dynamic result = null;

        try
        {
            result = await CacheReadThrough(cacheKey, () => GetEntryBaseCachingAsync(contentTypeUid, entryUid, _locale, _contentstackClient), logMessage, _logSource);
        }
        catch (Exception exception)
        {
            LogException(exception, logMessage);
        }

        return result;
    }

    private static async Task<dynamic> GetEntryBaseCachingAsync(string contentTypeUid, string entryUid, string locale, ContentstackClient contentstackClient)
    {
        if (string.IsNullOrEmpty(contentTypeUid))
            return null;
        if (string.IsNullOrEmpty(entryUid))
            return null;

        var entry = contentstackClient
            .ContentType(contentTypeUid)
            .Entry(entryUid)
            .SetLocale(locale)
            .includeEmbeddedItems();

        var task = entry.Fetch<dynamic>();
        var timeoutTask = Task.Delay(_timeout);
        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
            throw new TimeoutException();
        if (task.IsFaulted)
            throw task.Exception;

        return task.Result;
    }

    #endregion TODO - 4. MemoryCache - extract to base class (like BaseServiceLogger)

    #region TODO - 5. outgoing calls - limit to 5 concurrent

    public int GetQueuedCount()
    {
        // Return the number of tasks queued up
        return _maxConcurrency - _semaphore.CurrentCount;
    }

    public async Task<dynamic> GetEntryLimitConcurrentAsync(string contentTypeUid, string entryUid)
    {
        var logMessage = $"{_logMessageEntryPrefix} {{0}} {contentTypeUid} {entryUid}";
        var cacheKey = $"{_cacheKeyPrefix}:{contentTypeUid}:{entryUid}";
        dynamic result = null;

        await _semaphore.WaitAsync();

        try
        {
            // Perform the operation that should be limited
            Console.WriteLine("Processing started");

            // Not actually calling for this one, just simulating a delay
            await Task.Delay(TimeSpan.FromSeconds(2));

            Console.WriteLine("Processing completed");
        }
        finally
        {
            // Release the semaphore
            _semaphore.Release();
        }

        return $"Exit time: {DateTime.UtcNow.ToLongTimeString()}, In queue: {GetQueuedCount()}";
    }

    #endregion TODO - 5. outgoing calls - limit to 5 concurrent

    #region TODO - 6. Contentstack - add IncludeReference to outgoing call

    public async Task<dynamic> GetEntryIncludeReferenceAsync(string contentTypeUid, string entryUid, string? referenceName = null)
    {
        var logMessage = $"{_logMessageEntryPrefix} {{0}} {contentTypeUid} {entryUid}";
        var cacheKey = $"{_cacheKeyPrefix}:{contentTypeUid}:{entryUid}:{referenceName}";
        dynamic result = null;

        try
        {
            result = await CacheReadThrough(cacheKey, () => GetEntryIncludeReferenceAsync(contentTypeUid, entryUid, _locale, _contentstackClient, referenceName), logMessage, _logSource);
        }
        catch (Exception exception)
        {
            LogException(exception, logMessage);
        }

        return result;
    }

    private static async Task<dynamic> GetEntryIncludeReferenceAsync(string contentTypeUid, string entryUid, string locale, ContentstackClient contentstackClient, string referenceName)
    {
        if (string.IsNullOrEmpty(contentTypeUid))
            return null;
        if (string.IsNullOrEmpty(entryUid))
            return null;

        var entry = contentstackClient
            .ContentType(contentTypeUid)
            .Entry(entryUid)
            .SetLocale(locale)
            .includeEmbeddedItems();

        if (!string.IsNullOrEmpty(referenceName))
            entry.IncludeReference(referenceName);

        var task = entry.Fetch<dynamic>();
        var timeoutTask = Task.Delay(_timeout);
        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
            throw new TimeoutException();
        if (task.IsFaulted)
            throw task.Exception;

        return task.Result;
    }

    #endregion TODO - 6. Contentstack - add IncludeReference to outgoing call

    #region TODO - 7. Contentstack - introduce GetEntries method (with or without pagination?)

    public async Task<dynamic> GetEntriesAsync(string contentTypeUid, int? skip = null, int? take = null)
    {
        var logMessage = $"{_logMessageEntryPrefix} {{0}} {contentTypeUid}";
        var cacheKey = $"{_cacheKeyPrefix}:{contentTypeUid}:skip:{skip}:take:{take}";
        dynamic result = null;

        try
        {
            result = await CacheReadThrough(cacheKey, () => GetEntriesAsync(contentTypeUid, _locale, _contentstackClient, skip, take), logMessage, _logSource);
        }
        catch (Exception exception)
        {
            LogException(exception, logMessage);
        }

        return result;
    }

    private static async Task<dynamic> GetEntriesAsync(string contentTypeUid, string locale, ContentstackClient contentstackClient, int? skip = null, int? take = null)
    {
        var entry = contentstackClient
            .ContentType(contentTypeUid)
            .Query()
            .Descending("created_at")
            .SetLocale(locale)
            .IncludeCount()
            .includeEmbeddedItems();

        if (skip.HasValue)
            entry.Skip(skip.Value);
        if (take.HasValue)
            entry.Limit(take.Value);

        var task = entry.Find<dynamic>();
        var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(10_000));
        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
            throw new TimeoutException();
        if (task.IsFaulted)
            throw task.Exception;

        return task.Result;
    }

    #endregion TODO - 7. Contentstack - introduce GetEntries method (with or without pagination?)
}