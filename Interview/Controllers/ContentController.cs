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
        => JsonConvert.SerializeObject(_contentstackService.GetEntry(contentTypeUid, entryUid));
}
