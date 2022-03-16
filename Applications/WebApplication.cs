using System.Runtime.Versioning;
using HACC.Components;
using HACC.Models;
using HACC.Models.Drivers;
using Microsoft.Extensions.Logging;
using Terminal.Gui;

namespace HACC.Applications;

[SupportedOSPlatform(platformName: "browser")]
public class WebApplication
{
    public readonly WebClipboard WebClipboard;
    public readonly WebConsoleDriver WebConsoleDriver;
    public readonly WebMainLoopDriver WebMainLoopDriver;
    public WebApplication(ILogger logger, WebConsole console, TerminalSettings? terminalSettings = null)
    {
        this.WebClipboard = new WebClipboard();
        this.WebConsoleDriver = new WebConsoleDriver(
            logger: logger,
            webClipboard: this.WebClipboard,
            console: console,
            terminalSettings: terminalSettings);
        this.WebMainLoopDriver = new WebMainLoopDriver(() => FakeConsole.ReadKey(true));
    }

    public virtual void Init()
    {
        Application.Init(
            driver: this.WebConsoleDriver,
            mainLoopDriver: this.WebMainLoopDriver);
    }

    public virtual void Run()
    {
        Application.Run();
    }
}