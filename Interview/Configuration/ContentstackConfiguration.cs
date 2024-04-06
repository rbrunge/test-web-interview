using Contentstack.Core.Internals;

namespace Interview.Configuration;

public class ContentstackConfiguration
{
    public const string SectionName = "Contentstack";

    public const ContentstackRegion Region = ContentstackRegion.EU;

    public string ApiKey { get; set; }
    public string Environment { get; set; }
    public string DeliveryToken { get; set; }
    public string Locale { get; set; }
}
