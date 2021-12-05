using HACC;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using static HACC.Extensions.LoggingExtensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args: args);
builder.RootComponents.Add<App>(selector: "#app");
builder.RootComponents.Add<HeadOutlet>(selector: "head::after");

builder.Services.AddScoped(implementationFactory: sp => new HttpClient
    {BaseAddress = new Uri(uriString: builder.HostEnvironment.BaseAddress)});

builder.Services.AddOidcAuthentication(configure: options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind(key: "Local", instance: options.ProviderOptions);
});

builder.Logging.ClearProviders();
builder.Logging.AddCustomLogging(configure: configuration =>
{
    configuration.LogLevels.Add(
        key: LogLevel.Warning, value: ConsoleColor.DarkMagenta);
    configuration.LogLevels.Add(
        key: LogLevel.Error, value: ConsoleColor.Red);
});
builder.Logging.SetMinimumLevel(level: LogLevel.Debug);
await builder.Build().RunAsync();