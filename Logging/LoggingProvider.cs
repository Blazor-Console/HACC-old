using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace HACC.Logging;

public sealed class LoggingProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, CustomLogger> _loggers = new();
    private readonly IDisposable _onChangeToken;
    private LoggingConfiguration _currentConfig;

    public LoggingProvider(
        IOptionsMonitor<LoggingConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(listener: updatedConfig => _currentConfig = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(key: categoryName,
            valueFactory: name => new CustomLogger(name: name, getCurrentConfig: GetCurrentConfig));
    }

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken.Dispose();
    }

    private LoggingConfiguration GetCurrentConfig()
    {
        return _currentConfig;
    }
}