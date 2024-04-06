
namespace Interview.Services.Interfaces;

public interface IContentstackService
{
    dynamic GetEntry(string contentTypeUid, string entryUid);
}
