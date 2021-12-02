namespace HACC.Extensions
{
    using HACC.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Configuration;

    public static class LoggingExtensions
    {
        public static ILoggingBuilder AddCustomLogging(
            this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<ILoggerProvider, LoggingProvider>());

            LoggerProviderOptions.RegisterProviderOptions
                <LoggingConfiguration, LoggingProvider>(builder.Services);

            return builder;
        }

        public static ILoggingBuilder AddCustomLogging(
            this ILoggingBuilder builder,
            Action<LoggingConfiguration> configure)
        {
            builder.AddCustomLogging();
            builder.Services.Configure(configure);

            return builder;
        }
    }
}
