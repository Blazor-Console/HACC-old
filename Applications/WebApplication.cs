using System.Runtime.Versioning;
using HACC.Components;
using HACC.Models;
using HACC.Models.Drivers;
using Microsoft.Extensions.Logging;
using Terminal.Gui;

namespace HACC.Applications;

public class WebApplication
{
    public readonly WebConsoleDriver WebConsoleDriver;
    public readonly WebMainLoopDriver WebMainLoopDriver;

    public WebApplication(ILogger logger, TerminalSettings? terminalSettings = null)
    {
        this.WebConsoleDriver = new WebConsoleDriver(
            logger: logger,
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

    public virtual void Shutdown()
    {
        Application.Shutdown();
    }
}