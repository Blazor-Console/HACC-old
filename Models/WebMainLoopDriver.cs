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
public class WebMainLoopDriver : IMainLoopDriver
{
    private readonly Func<ConsoleKeyInfo> _consoleKeyReaderFn;

    private readonly AutoResetEvent _keyReady = new(initialState: false);
    private readonly AutoResetEvent _waitForProbe = new(initialState: false);

    private ConsoleKeyInfo? _keyResult;
    private MainLoop _mainLoop;

    /// <summary>
    ///     Invoked when a Key is pressed.
    /// </summary>
    public Action<ConsoleKeyInfo> KeyPressed;


    /// <summary>
    ///     Creates a new Mainloop.
    /// </summary>
    /// <param name="driver">
    ///     Should match the <see cref="ConsoleDriver" /> (one of the implementations UnixMainLoop,
    ///     NetMainLoop or WindowsMainLoop).
    /// </param>
    /// <param name="consoleKeyReaderFn"></param>
    public WebMainLoopDriver(Func<ConsoleKeyInfo>? consoleKeyReaderFn = null)
    {
        this._consoleKeyReaderFn =
            consoleKeyReaderFn ?? throw new ArgumentNullException(paramName: nameof(consoleKeyReaderFn));
    }

    void IMainLoopDriver.Setup(MainLoop mainLoop)
    {
        this._mainLoop = mainLoop ?? throw new ArgumentException(message: "MainLoop must be provided");
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
        if (this._mainLoop.timeouts.Count > 0)
        {
            waitTimeout = (int) ((this._mainLoop.timeouts.Keys[index: 0] - now) / TimeSpan.TicksPerMillisecond);
            if (waitTimeout < 0)
                return true;
        }
        else
        {
            waitTimeout = -1;
        }

        if (!wait)
            waitTimeout = 0;

        this._keyResult = null;
        this._waitForProbe.Set();
        this._keyReady.WaitOne(millisecondsTimeout: waitTimeout);
        return this._keyResult.HasValue;
    }

    void IMainLoopDriver.MainIteration()
    {
        if (!this._keyResult.HasValue) return;
        this.KeyPressed?.Invoke(obj: this._keyResult.Value);
        this._keyResult = null;
    }

    private void ConsoleKeyReader()
    {
        while (true)
        {
            this._waitForProbe.WaitOne();
            this._keyResult = this._consoleKeyReaderFn();
            this._keyReady.Set();
        }
    }
}