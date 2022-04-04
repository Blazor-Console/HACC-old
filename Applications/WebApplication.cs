using HACC.Components;
using HACC.Models;
using HACC.Models.Drivers;
using Terminal.Gui;

namespace HACC.Applications;

public class WebApplication
{
    private Application.RunState state;
    private bool wait = true;
    private bool _initialized;

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
        Application.RunIteration(state, wait, false);
    }

    public virtual void Init()
    {
        if (_initialized) return;

        Application.Init(
            driver: this.WebConsoleDriver,
            mainLoopDriver: this.WebMainLoopDriver);
        _initialized = true;
    }

    public virtual void Run(Func<Exception, bool> errorHandler = null)
    {
        Run(Application.Top, errorHandler);
    }

    public virtual void Run<T>(Func<Exception, bool> errorHandler = null) where T : Toplevel, new()
    {
        if (_initialized && Application.Driver != null)
        {
            var top = new T();
            if (top.GetType().BaseType != typeof(Toplevel))
            {
                throw new ArgumentException(top.GetType().BaseType.Name);
            }
            Run(top, errorHandler);
        }
        else
        {
            Init();
            Run(Application.Top, errorHandler);
        }
    }

    public virtual void Run(Toplevel view, Func<Exception, bool> errorHandler = null)
    {
        try
        {
            if (!_initialized)
                Init();

            _ = Task.Run(() => state = Application.Begin(toplevel: view ?? Application.Top));
            _ = Task.Run(() => WebConsoleDriver.firstRender = false);
            _ = Task.Run(Application.Refresh);
            _ = Task.Run(() => Application.RunIteration(state, wait, true));
        }
        catch (Exception error)
        {
            if (errorHandler == null)
            {
                throw;
            }
            if (!errorHandler(error))
                Shutdown();
        }
    }

    public virtual void End(Application.RunState runState = null)
    {
        if (!_initialized) return;

        if (runState != null || state != null)
            Application.End(runState ?? state);
    }

    public virtual void Shutdown()
    {
        Application.Shutdown();
        _initialized = false;
    }
}