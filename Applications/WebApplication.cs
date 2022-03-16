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
    public readonly WebClipboard WebClipboard;
    public readonly WebConsole WebConsole;
    public readonly WebMainLoopDriver WebMainLoopDriver;
    public WebApplication(ILogger logger, Console console, TerminalSettings? terminalSettings = null)
    {
        this.WebClipboard = new WebClipboard();
        this.WebConsole = new WebConsole(
            logger: logger,
            console: console,
            webClipboard: this.WebClipboard,
            terminalSettings: terminalSettings);
        this.WebMainLoopDriver = new WebMainLoopDriver(() => FakeConsole.ReadKey(true));
    }

    public virtual void Init()
    {
        Application.Init(
            driver: this.WebConsole,
            mainLoopDriver: this.WebMainLoopDriver);
    }

    public virtual void Run()
    {
        Application.Run();
    }
}