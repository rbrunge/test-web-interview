namespace Interview.Services.Interfaces;

public interface IContentstackService
{
    Task<dynamic> GetEntriesAsync(string contentTypeUid, int? skip = null, int? take = null);

    dynamic GetEntry(string contentTypeUid, string entryUid);

    Task<dynamic> GetEntryAsync(string contentTypeUid, string entryUid);

    Task<dynamic> GetEntryBaseCachingAsync(string contentTypeUid, string entryUid);

    Task<dynamic> GetEntryIncludeReferenceAsync(string contentTypeUid, string entryUid, string referenceName = null);

    Task<dynamic> GetEntryLimitConcurrentAsync(string contentTypeUid, string entryUid);

    dynamic GetEntryStatic(string contentTypeUid, string entryUid);

    int GetQueuedCount();
}