namespace HACC.Logging
{
    using System.Collections.Concurrent;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public sealed class LoggingProvider : ILoggerProvider
    {
        private readonly IDisposable _onChangeToken;
        private LoggingConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, CustomLogger> _loggers = new();

        public LoggingProvider(
            IOptionsMonitor<LoggingConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName) =>
            _loggers.GetOrAdd(categoryName, name => new CustomLogger(name, GetCurrentConfig));

        private LoggingConfiguration GetCurrentConfig() => _currentConfig;

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken.Dispose();
        }
    }
}
