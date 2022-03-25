using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Logging;
using static Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions;

namespace HACC.Extensions;

public static class HaccExtensions
{
    public static void UsingHacc(this WebAssemblyHostBuilder builder)
    {
        builder.Services.AddScoped(implementationFactory: sp => new HttpClient
            {BaseAddress = new Uri(uriString: builder.HostEnvironment.BaseAddress)});

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
        
        //builder.Services.AddSingleton(implementationFactory: serviceProvider => new WebApplication(webConsoleDriver: new WebConsoleDriver(
        //    logger: serviceProvider.GetService<ILoggerFactory>()!.CreateLogger("Logging"))));
        //builder.Services.AddSingleton(implementationFactory: serviceProvider => new WebConsole(
        //    logger: serviceProvider.GetService<ILoggerFactory>()!.CreateLogger("Logging")));
        //builder.Services.AddSingleton(implementationFactory: serviceProvider => new WebApplication(
        //    logger: serviceProvider.GetService<ILoggerFactory>()!.CreateLogger("Logging")));
    }
}