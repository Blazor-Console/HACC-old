using HACC;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using static HACC.Extensions.LoggingExtensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});

builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    builder.Configuration.Bind("Local", options.ProviderOptions);
});

builder.Logging.ClearProviders();
builder.Logging.AddCustomLogging(configuration =>
{
    configuration.LogLevels.Add(
        LogLevel.Warning, ConsoleColor.DarkMagenta);
    configuration.LogLevels.Add(
        LogLevel.Error, ConsoleColor.Red);
});
builder.Logging.SetMinimumLevel(LogLevel.Debug);
await builder.Build().RunAsync();