using HACC.Applications;
using HACC.Models;
using HACC.Models.Drivers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HACC.Extensions;

public static class HaccExtensions
{
    private static ServiceProvider? _serviceProvider;
    private static ILoggerFactory? _loggerFactory;

    public static ServiceProvider ServiceProvider =>
        _serviceProvider ?? throw new InvalidOperationException(message: "Call UseHacc() first");

    public static ILoggerFactory LoggerFactory =>
        _loggerFactory ?? throw new InvalidOperationException(message: "Call UseHacc() first");

    public static ILogger<T> CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
    public static WebAssemblyHostBuilder UseHacc(this WebAssemblyHostBuilder builder)
    {
        builder.Logging.ClearProviders();
        builder.Logging.AddCustomLogging(configure: configuration =>
        {
            configuration.LogLevels.Add(
                key: LogLevel.Warning,
                value: ConsoleColor.DarkMagenta);
            configuration.LogLevels.Add(
                key: LogLevel.Error,
                value: ConsoleColor.Red);
        });
        builder.Logging.SetMinimumLevel(level: LogLevel.Debug);

        _serviceProvider = builder.Services.BuildServiceProvider();
        _loggerFactory = _serviceProvider.GetService<ILoggerFactory>()!;
        builder.Services.AddSingleton(implementationFactory: serviceProvider => new WebClipboard());
        var webConsoleDriver = new WebConsoleDriver();
        builder.Services.AddSingleton(implementationFactory: serviceProvider => webConsoleDriver);
        builder.Services.AddSingleton(implementationFactory: serviceProvider => new WebMainLoopDriver());
        builder.Services.AddSingleton(implementationFactory: serviceProvider => new WebApplication());
        return builder;
    }

    public static T GetService<T>()
    {
        return ServiceProvider.GetService<T>()!;
    }
}