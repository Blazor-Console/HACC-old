using HACC.Applications;
using HACC.Components;
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
        var webClipboard = new WebClipboard();
        var webConsole = new WebConsole();
        var webConsoleDriver = new WebConsoleDriver(webClipboard: webClipboard);
        var webMainLoopDriver = new WebMainLoopDriver(webConsoleDriver: webConsoleDriver);
        builder.Services.AddSingleton<WebClipboard>();
        builder.Services.AddSingleton<WebConsoleDriver>(implementationInstance: webConsoleDriver);
        builder.Services.AddSingleton<WebMainLoopDriver>(implementationInstance: webMainLoopDriver);
        builder.Services.AddSingleton<WebApplication>(implementationInstance: new WebApplication(
            webConsoleDriver: webConsoleDriver,
            webMainLoopDriver: webMainLoopDriver));
        return builder;
    }

    public static T GetService<T>()
    {
        return ServiceProvider.GetService<T>()!;
    }
}