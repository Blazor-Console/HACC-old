using System.Runtime.Versioning;
using HACC.Models;
using HACC.Models.Drivers;
using Microsoft.Extensions.Logging;
using Terminal.Gui;
using Console = HACC.Components.Console;

namespace HACC.Applications;

[SupportedOSPlatform(platformName: "browser")]
public class WebApplication
{
    public WebApplication(ILogger logger, Console console, TerminalSettings? terminalSettings = null)
    {
        var webClipboard = new WebClipboard();
        var webConsole = new WebConsole(
            logger: logger,
            console: console,
            webClipboard: webClipboard,
            terminalSettings: terminalSettings);
        var webMainLoopDriver = new WebMainLoopDriver();
        Application.Init(driver: webConsole, mainLoopDriver: webMainLoopDriver);
    }
}