using Contentstack.Core;
using Contentstack.Core.Configuration;
using Contentstack.Core.Internals;
using Interview.Configuration;
using Interview.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace InterviewTests.Integration;

[TestClass, TestCategory("integration")]
public class ContentstackServiceTests
{
    private ContentstackOptions _contentstackOptions = null!;
    private ContentstackClient _contentstackClient = null!;
    private string _locale = null!;

    [TestInitialize]
    public void Initialize()
    {
        // Create a configuration object
        var config = new ConfigurationBuilder()
            .AddUserSecrets<ContentstackServiceTests>()
            .Build();

        _contentstackOptions = new ContentstackOptions
        {
            DeliveryToken = config["Contentstack:DeliveryToken"],
            ApiKey = config["Contentstack:ApiKey"],
            Environment = config["Contentstack:Environment"],
            Region = ContentstackConfiguration.Region
        };

        _contentstackClient = new ContentstackClient(_contentstackOptions);

        _locale = config["Contentstack:Locale"]!;
    }

    private ContentstackService GetContentstackService()
    {
        var logger = Substitute.For<ILogger<ContentstackService>>();
        var memoryCache = Substitute.For<IMemoryCache>();
        var options = Substitute.For<IOptions<ContentstackConfiguration>>();
        options.Value.Returns(new ContentstackConfiguration
        {
            ApiKey = _contentstackOptions.ApiKey,
            DeliveryToken = _contentstackOptions.DeliveryToken,
            Environment = _contentstackOptions.Environment,
            Locale = _locale
        });

        return new ContentstackService(logger, memoryCache, options);
    }

    [TestMethod]
    public void ContentstackService_GetEntry_Ok()
    {
        // Arrange
        var contentstackService = GetContentstackService();

        (string ModelUid, string EntryUid) entry = _contentstackOptions.Environment.Equals("preview")
            ? ("home_page", "bltc6f07f8ae4bf7bb9") //
            : ("clever_public_ct_youtube_video", "blt56330984c515aaea"); // clever_public_ct_case

        // Act
        var result = contentstackService.GetEntry(entry.ModelUid, entry.EntryUid);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void ContentstackService_GetEntryStatic_Ok()
    {
        // Arrange
        var contentstackService = GetContentstackService();

        (string ModelUid, string EntryUid) entry = _contentstackOptions.Environment.Equals("preview")
            ? ("home_page", "bltc6f07f8ae4bf7bb9")
            : ("clever_public_ct_youtube_video", "blt56330984c515aaea");

        // Act
        var result = contentstackService.GetEntryStatic(entry.ModelUid, entry.EntryUid);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ContentstackService_GetEntryAsync_Ok()
    {
        // Arrange
        var contentstackService = GetContentstackService();

        (string ModelUid, string EntryUid) entry = _contentstackOptions.Environment.Equals("preview")
            ? ("home_page", "bltc6f07f8ae4bf7bb9")
            : ("clever_public_ct_youtube_video", "blt56330984c515aaea");

        // Act
        var result = await contentstackService.GetEntryAsync(entry.ModelUid, entry.EntryUid);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ContentstackService_GetEntryBaseCachingAsync_Ok()
    {
        // Arrange
        var contentstackService = GetContentstackService();

        (string ModelUid, string EntryUid) entry = _contentstackOptions.Environment.Equals("preview")
            ? ("home_page", "bltc6f07f8ae4bf7bb9")
            : ("clever_public_ct_youtube_video", "blt56330984c515aaea");

        // Act
        var result = await contentstackService.GetEntryBaseCachingAsync(entry.ModelUid, entry.EntryUid);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ContentstackService_GetEntryLimitAsync_Ok()
    {
        // Arrange
        var myService = GetContentstackService();
        const int maxConcurrency = 5;

        // Act
        // Start 7 asynchronous tasks simultaneously
        var tasks = new Task[maxConcurrency + 13];
        for (int i = 0; i < tasks.Length; i++)
        {
            if (i > 8)
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            tasks[i] = myService.GetEntryLimitConcurrentAsync("Lorem", "Ipsum"); ;
        }

        // Assert
        // Wait for a few seconds to observe semaphore count changes
        for (int i = 1; i <= 10; i++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1000));
            Console.WriteLine($"After {i} second(s), numbers in queue: {myService.GetQueuedCount()}");
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks);

        // Output the final semaphore count
        Console.WriteLine($"Final semaphore count: {myService.GetQueuedCount()}");
    }

    [TestMethod]
    public async Task ContentstackService_GetEntryIncludeReference_Ok()
    {
        // Arrange
        var contentstackService = GetContentstackService();

        (string ModelUid, string EntryUid, string ReferenceName) entry = _contentstackOptions.Environment.Equals("preview")
            ? ("bob_content_type", "blt21211add92fbb40a", "reference")
            : ("customer_onboarding_modal_module", "blt5fd48487060a9925", "reference");

        // Act 1
        var result1 = await contentstackService.GetEntryIncludeReferenceAsync(entry.ModelUid, entry.EntryUid);

        // Assert 1
        Assert.IsNotNull(result1);
        Assert.AreEqual(2, ((result1 as Newtonsoft.Json.Linq.JObject).Properties().FirstOrDefault(x => x.Name == "reference").Value as Newtonsoft.Json.Linq.JArray).FirstOrDefault().Count());

        // Act 2
        var result2 = await contentstackService.GetEntryIncludeReferenceAsync(entry.ModelUid, entry.EntryUid, entry.ReferenceName);

        // Assert 2
        Assert.IsTrue(((result2 as Newtonsoft.Json.Linq.JObject).Properties().FirstOrDefault(x => x.Name == "reference").Value as Newtonsoft.Json.Linq.JArray).FirstOrDefault().Count() > 2);
    }

    [TestMethod]
    public async Task ContentstackService_SkipTake_Ok()
    {
        // Arrange
        var contentstackService = GetContentstackService();

        string modelUid = _contentstackOptions.Environment.Equals("preview")
            ? "article"
            : "clever_public_ct_page";

        // Act 1
        var result1 = await contentstackService.GetEntriesAsync(modelUid);

        // Assert 1
        Assert.IsNotNull(result1);
        Assert.IsTrue((result1 as Contentstack.Core.Models.ContentstackCollection<Object>).Count() > 1);

        // Act 2
        var result2 = await contentstackService.GetEntriesAsync(modelUid, take: 7);

        // Assert 2
        Assert.IsNotNull(result2);
        Assert.IsTrue((result2 as Contentstack.Core.Models.ContentstackCollection<Object>).Count() == 7);

        // Act 3
        var result3 = await contentstackService.GetEntriesAsync(modelUid, skip: 200, take: 10);

        // Assert 3
        Assert.IsNotNull(result3);
        Assert.IsTrue((result3 as Contentstack.Core.Models.ContentstackCollection<Object>).Count() == 0);
    }
}