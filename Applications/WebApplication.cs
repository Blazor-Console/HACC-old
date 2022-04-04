using HACC.Components;
using HACC.Models;
using HACC.Models.Drivers;
using Terminal.Gui;

namespace HACC.Applications;

public class WebApplication
{
    private Application.RunState state;

    public readonly WebConsoleDriver WebConsoleDriver;
    public readonly WebMainLoopDriver WebMainLoopDriver;
    public readonly WebConsole WebConsole;

    public WebApplication(WebConsoleDriver webConsoleDriver, 
        WebMainLoopDriver webMainLoopDriver, WebConsole webConsole)
    {
        this.WebConsoleDriver = webConsoleDriver;
        // TODO: we should be able to implement something that reads from the actual key events set up in WebConsole.razor for key press events on the console
        // Maybe from the Canvas2DContext StdIn
        this.WebMainLoopDriver = webMainLoopDriver;
        this.WebConsole = webConsole;
        this.WebConsole.RunIterationNeeded += WebConsole_RunIterationNeeded;
    }

    private void WebConsole_RunIterationNeeded()
    {
        Application.RunIteration(state, true, false);
    }

    public virtual void Init()
    {
        Application.Init(
            driver: this.WebConsoleDriver,
            mainLoopDriver: this.WebMainLoopDriver);
    }

    public virtual void Run()
    {
        _ = Task.Run(() => state = Application.Begin(toplevel: Application.Top));
        _ = Task.Run(() => WebConsoleDriver.firstRender = false);
        _ = Task.Run(Application.Refresh);
    }

    public virtual void Shutdown()
    {
        Application.Shutdown();
    }
}