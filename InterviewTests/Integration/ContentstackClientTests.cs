using Contentstack.Core;
using Contentstack.Core.Configuration;
using Contentstack.Core.Internals;
using Contentstack.Core.Models;
using Interview.Configuration;
using Microsoft.Extensions.Configuration;

namespace InterviewTests.Integration;

[TestClass, TestCategory("integration")]
public class ContentstackClientTests
{
    private ContentstackOptions _contentstackOptions = null!;
    private ContentstackClient _contentstackClient = null!;
    private string _locale = null!;

    [TestInitialize]
    public void Initialize()
    {
        // Create a configuration object
        var config = new ConfigurationBuilder()
            .AddUserSecrets<ContentstackClientTests>()
            .Build();

        _contentstackOptions = new ContentstackOptions
        {
            DeliveryToken = config["Contentstack:DeliveryToken"],
            ApiKey = config["Contentstack:ApiKey"],
            Environment = config["Contentstack:Environment"],
            Region = ContentstackConfiguration.Region,
            Host = null,
            Timeout = 10_000,
        };

        _contentstackClient = new ContentstackClient(_contentstackOptions);

        _locale = config["Contentstack:Locale"]!;
    }

    [TestMethod]
    public async Task ContentstackClient_GetContentTypes_OK()
    {
        var contentTypes = await _contentstackClient.GetContentTypes();
        Assert.IsNotNull(contentTypes);
    }

    [TestMethod]
    public async Task ContentstackClient_GetMultipleEntriesOfContentType_OK()
    {
        var contentTypeUid = _contentstackOptions.Environment.Equals("preview")
            ? "bob_content_type"
            : "customer_onboarding_modal_module";

        var query = _contentstackClient.ContentType(contentTypeUid).Query()
            // .Exists("reference")
            .IncludeSchema()
            .IncludeCount()
            .SetLocale(_locale);
        var task = query.Find<dynamic>();
        var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(10_000));
        var completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromMilliseconds(10_000)));

        if (completedTask == timeoutTask)
            throw new TimeoutException();

        if (task.IsFaulted)
            throw task.Exception;

        var result = task.Result;

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ContentstackClient_GetEntryContent_OK()
    {
        (string ModelUid, string EntryUid) entry = _contentstackOptions.Environment.Equals("preview")
            ? ("home_page", "bltc6f07f8ae4bf7bb9") 
            : ("clever_public_ct_youtube_video", "blt56330984c515aaea");

        var entryContent = await GetEntryAsync(entry.ModelUid, entry.EntryUid, _locale, _contentstackClient);
        Assert.IsNotNull(entryContent);
    }

    [TestMethod]
    public async Task ContentstackClient_GetEntryIncludeReferenceContent_OK()
    {
        // Arrange
        (string ModelUid, string EntryUid, string EntryUid2) entryInput = _contentstackOptions.Environment.Equals("preview")
            ? ("bob_content_type", "blt21211add92fbb40a", "reference") // WORKS
            : ("customer_onboarding_modal_module", "blt5fd48487060a9925", "reference"); // Works, attributes for video

        Entry entry2 = await _contentstackClient.ContentType(entryInput.ModelUid).Entry(entryInput.EntryUid).IncludeReference(entryInput.EntryUid2).Fetch<Entry>();

        // Act
        var entry = _contentstackClient
            .ContentType(entryInput.ModelUid)
            .Entry(entryInput.EntryUid)
            .SetLocale(_locale)
            .IncludeReference(entryInput.EntryUid2)
            ;

        var task = entry.Fetch<dynamic>();
        var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(10_000));
        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
            throw new TimeoutException();
        if (task.IsFaulted)
            throw task.Exception;

        var result = task.Result;

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ContentstackClient_SkipTake_OK()
    {
        string contentType = _contentstackOptions.Environment.Equals("preview")
            ? "article" //
            : "clever_public_ct_page";

        // Act 1
        var result1 = (ContentstackCollection<Object>)await GetEntriesAsync(contentType, _locale, _contentstackClient, 0, 1);

        // Assert 1
        Assert.IsNotNull(result1);
        Assert.AreEqual(result1.Count(), 1);

        // Act 1
        var result2 = (ContentstackCollection<Object>)await GetEntriesAsync(contentType, _locale, _contentstackClient, 2, 3);

        // Assert 1
        Assert.IsNotNull(result2);
        Assert.AreEqual(result2.Count(), 3);
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
        var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(10_000));
        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask == timeoutTask)
            throw new TimeoutException();
        if (task.IsFaulted)
            throw task.Exception;

        return task.Result;
    }

    private static async Task<dynamic> GetEntriesAsync(string contentTypeUid, string locale, ContentstackClient contentstackClient, int? skip = null, int? take = null)
    {
        var entry = contentstackClient
            .ContentType(contentTypeUid)
            .Query()
            .Descending("created_at")
            .SetLocale(locale)
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
}