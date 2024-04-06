using Interview.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Interview.Controllers;

[ApiController]
[Route("[controller]")]
public class ContentController : ControllerBase
{
    private readonly IContentstackService _contentstackService;

    public ContentController(IContentstackService contentstackService)
    { _contentstackService = contentstackService; }

    [HttpGet("entry/{contentTypeUid}/{entryUid}")]
    public string Entry(string contentTypeUid, string entryUid)
        => JsonConvert.SerializeObject(_contentstackService.GetEntry(contentTypeUid, entryUid), Formatting.Indented);

    [HttpGet("entry-static/{contentTypeUid}/{entryUid}")]
    public string EntryStatic(string contentTypeUid, string entryUid)
        => JsonConvert.SerializeObject(_contentstackService.GetEntryStatic(contentTypeUid, entryUid), Formatting.Indented);

    [HttpGet("entry-async/{contentTypeUid}/{entryUid}")]
    public async Task<string> EntryAsync(string contentTypeUid, string entryUid)
        => JsonConvert.SerializeObject(await _contentstackService.GetEntryAsync(contentTypeUid, entryUid), Formatting.Indented);

    [HttpGet("entry-base-caching/{contentTypeUid}/{entryUid}")]
    public async Task<string> EntryBaseCaching(string contentTypeUid, string entryUid)
        => JsonConvert.SerializeObject(await _contentstackService.GetEntryBaseCachingAsync(contentTypeUid, entryUid), Formatting.Indented);

    [HttpGet("entry-limit/{contentTypeUid}/{entryUid}")]
    public async Task<string> EntryLimit(string contentTypeUid, string entryUid)
        => JsonConvert.SerializeObject(await _contentstackService.GetEntryLimitConcurrentAsync(contentTypeUid, entryUid), Formatting.Indented);

    [HttpGet("entry-include-reference/{contentTypeUid}/{entryUid}/{referenceName}")]
    public async Task<string> EntryIncludeReference(string contentTypeUid, string entryUid, string? referenceName)
        => JsonConvert.SerializeObject(await _contentstackService.GetEntryIncludeReferenceAsync(contentTypeUid, entryUid, referenceName), Formatting.Indented);

    [HttpGet("entry-skiptake/{contentTypeUid}")]
    public async Task<string> EntryIncludeReference(string contentTypeUid, int? skip = null, int? take = null)
        => JsonConvert.SerializeObject(await _contentstackService.GetEntriesAsync(contentTypeUid, skip, take), Formatting.Indented);
}