
namespace Interview.Services.Base;

public abstract class BaseServiceLogger
{
    private const string _source = "Service";
    private const string _messageFound = "From {0}";
    private const string _messageNotFound = "Not Found";
    private const string _messageFoundCached = "From Cache";
    private const string _messageException = "Exception";

    private readonly ILogger _logger;

    protected BaseServiceLogger(ILogger logger)
    {
        _logger = logger;
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
}
