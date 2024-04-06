using Interview.Configuration;
using Interview.Services.Base;
using Interview.Services.Interfaces;
using Contentstack.Core;
using Contentstack.Core.Internals;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Interview.Services;

public class ContentstackService : BaseServiceLogger, IContentstackService
{
    // TODO - hardcoded values - introduce const for each
    // TODO - private GetEntry - make static
    // TODO - public GetEntry - make async
    // TODO - MemoryCache - extract to base class (like BaseServiceLogger)
    // TODO - outgoing calls - limit to 5 concurrent
    // TODO - Contentstack - add IncludeReference to outgoing call
    // TODO - Contentstack - introduce GetEntries method (with or without pagination?)
    // TODO - Contentstack - support live preview

    private const int _timeoutSeconds = 60;
    private const int _cacheSeconds = 60;
    private const string _logSource = "Stack";
    private const string _logMessageEntryPrefix = "Entry";
    private const string _cacheKeyPrefix = "Contentstack";

    private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(_timeoutSeconds);

    private readonly IMemoryCache _memoryCache;
    private readonly ContentstackClient _contentstackClient;

    public ContentstackService(ILogger<ContentstackService> logger, IMemoryCache memoryCache, IOptions<ContentstackConfiguration> contentstackConfiguration)
        : base(logger)
    {
        _memoryCache = memoryCache;
        var apiKey = contentstackConfiguration.Value.ApiKey;
        var deliveryToken = contentstackConfiguration.Value.DeliveryToken;
        var environment = contentstackConfiguration.Value.Environment;
        _contentstackClient = new ContentstackClient(apiKey, deliveryToken, environment, null, ContentstackRegion.EU);
    }


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
            result = GetEntry(contentTypeUid, entryUid, "da-dk");

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
}
