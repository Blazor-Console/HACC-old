using System.Runtime.Versioning;
using Terminal.Gui;

namespace HACC.Models;

//
// MainLoop.cs: IMainLoopDriver and MainLoop for Terminal.Gui
//
// Authors:
//   Miguel de Icaza (miguel@gnome.org)
//

/// <summary>
///     Simple main loop implementation that can be used to monitor
///     file descriptor, run timers and idle handlers.
/// </summary>
/// <remarks>
///     Monitoring of file descriptors is only available on Unix, there
///     does not seem to be a way of supporting this on Windows.
/// </remarks>
[SupportedOSPlatform(platformName: "browser")]
public class WebMainLoopDriver : IMainLoopDriver
{
    private readonly Func<ConsoleKeyInfo> consoleKeyReaderFn;

    private readonly AutoResetEvent keyReady = new(initialState: false);
    private readonly AutoResetEvent waitForProbe = new(initialState: false);
    private List<Func<bool>> _idleHandlers = new();

    /// <summary>
    ///     Invoked when a Key is pressed.
    /// </summary>
    public Action<ConsoleKeyInfo> KeyPressed;

    private ConsoleKeyInfo? keyResult;
    private MainLoop mainLoop;


    /// <summary>
    ///     Creates a new Mainloop.
    /// </summary>
    /// <param name="driver">
    ///     Should match the <see cref="ConsoleDriver" /> (one of the implementations UnixMainLoop,
    ///     NetMainLoop or WindowsMainLoop).
    /// </param>
    public WebMainLoopDriver(Func<ConsoleKeyInfo> consoleKeyReaderFn = null)
    {
        if (consoleKeyReaderFn == null)
            throw new ArgumentNullException(paramName: "key reader function must be provided.");
        this.consoleKeyReaderFn = consoleKeyReaderFn;
    }

    void IMainLoopDriver.Setup(MainLoop mainLoop)
    {
        if (mainLoop is null) throw new ArgumentException(message: "MainLoop must be provided");

        this.mainLoop =mainLoop;
        //var readThread = new Thread(start: this.ConsoleKeyReader);
        //readThread.Start();
        //Task.Run(action: this.ConsoleKeyReader);
    }

    void IMainLoopDriver.Wakeup()
    {
    }

    bool IMainLoopDriver.EventsPending(bool wait)
    {
        var now = DateTime.UtcNow.Ticks;

        int waitTimeout;
        if (this.mainLoop.Timeouts.Count > 0)
        {
            waitTimeout = (int) ((this.mainLoop.Timeouts.Keys[index: 0] - now) / TimeSpan.TicksPerMillisecond);
            if (waitTimeout < 0)
                return true;
        }
        else
        {
            waitTimeout = -1;
        }

        if (!wait)
            waitTimeout = 0;

        this.keyResult = null;
        this.waitForProbe.Set();
        this.keyReady.WaitOne(millisecondsTimeout: waitTimeout);
        return this.keyResult.HasValue;
    }

    void IMainLoopDriver.MainIteration()
    {
        if (!this.keyResult.HasValue) return;
        this.KeyPressed?.Invoke(obj: this.keyResult.Value);
        this.keyResult = null;
    }

    private void ConsoleKeyReader()
    {
        while (true)
        {
            this.waitForProbe.WaitOne();
            this.keyResult = this.consoleKeyReaderFn();
            this.keyReady.Set();
        }
    }
}