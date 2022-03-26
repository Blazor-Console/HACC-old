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

    private static WebApplication? _webApplication;
    private static WebClipboard? _webClipboard;
    private static WebConsoleDriver? _webConsoleDriver;
    private static WebMainLoopDriver? _webMainLoopDriver;

    private const string DefaultError = "Call UseHacc() first";
    public static WebApplication WebApplication =>
        _webApplication ?? throw new InvalidOperationException(message: DefaultError);

    public static WebClipboard WebClipboard =>
        _webClipboard ?? throw new InvalidOperationException(message: DefaultError);

    public static WebConsoleDriver WebConsoleDriver =>
        _webConsoleDriver ?? throw new InvalidOperationException(message: DefaultError);

    public static WebMainLoopDriver WebMainLoopDriver =>
        _webMainLoopDriver ?? throw new InvalidOperationException(message: DefaultError);

    public static ServiceProvider ServiceProvider =>
        _serviceProvider ?? throw new InvalidOperationException(message: DefaultError);

    public static ILoggerFactory LoggerFactory =>
        _loggerFactory ?? throw new InvalidOperationException(message: DefaultError);

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
        var webConsoleDriver = new WebConsoleDriver(webClipboard: webClipboard);
        var webMainLoopDriver = new WebMainLoopDriver(webConsoleDriver: webConsoleDriver);
        builder.Services.AddSingleton<WebClipboard>(implementationInstance: webClipboard);
        builder.Services.AddSingleton<WebConsoleDriver>(implementationInstance: webConsoleDriver);
        builder.Services.AddSingleton<WebMainLoopDriver>(implementationInstance: webMainLoopDriver);
        _webApplication = new WebApplication(
            webConsoleDriver: webConsoleDriver,
            webMainLoopDriver: webMainLoopDriver);
        builder.Services.AddSingleton<WebApplication>(implementationInstance: _webApplication);
        return builder;
    }

    public static T GetService<T>()
    {
        return ServiceProvider.GetService<T>()!;
    }
}