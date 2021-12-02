namespace HACC.Logging
{
    using Microsoft.Extensions.Logging;
    public class LoggingConfiguration
    {
        public int EventId { get; set; }

        public Dictionary<LogLevel, ConsoleColor> LogLevels { get; set; } = new()
        {
            [LogLevel.Information] = ConsoleColor.Green
        };
    }
}
