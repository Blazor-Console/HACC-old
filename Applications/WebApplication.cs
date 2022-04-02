using HACC.Models;
using HACC.Models.Drivers;
using Terminal.Gui;

namespace HACC.Applications;

public class WebApplication
{
    public readonly WebConsoleDriver WebConsoleDriver;
    public readonly WebMainLoopDriver WebMainLoopDriver;

    public WebApplication(WebConsoleDriver webConsoleDriver, WebMainLoopDriver webMainLoopDriver)
    {
        this.WebConsoleDriver = webConsoleDriver;
        // TODO: we should be able to implement something that reads from the actual key events set up in WebConsole.razor for key press events on the console
        // Maybe from the Canvas2DContext StdIn
        //this.WebMainLoopDriver = new WebMainLoopDriver(() => FakeConsole.ReadKey(true));
        this.WebMainLoopDriver = webMainLoopDriver;
    }

    public virtual void Init()
    {
        Application.Init(
            driver: this.WebConsoleDriver,
            mainLoopDriver: this.WebMainLoopDriver);
    }

    public virtual void Run()
    {
        //Application.Run();
        Application.Begin(toplevel: Application.Top);
        WebConsoleDriver.firstRender = false;
        Task.Run(Application.Refresh);
    }

    public virtual void Shutdown()
    {
        Application.Shutdown();
    }
}