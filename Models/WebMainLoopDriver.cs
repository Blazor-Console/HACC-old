using HACC.Components;
using HACC.Models.Structs;
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
    private readonly ManualResetEventSlim _keyReady = new ManualResetEventSlim(initialState: false);

    private readonly WebConsole _webConsole;

    Queue<InputResult?> _inputResult = new Queue<InputResult?>();
    private MainLoop _mainLoop;
    CancellationTokenSource tokenSource = new CancellationTokenSource();

    /// <summary>
    ///     Invoked when a Key is pressed, mouse is clicked or on resizing.
    /// </summary>
    public Action<InputResult> ProcessInput;


    /// <summary>
    ///     Creates a new Mainloop.
    /// </summary>
    /// <param name="driver">
    ///     Should match the <see cref="ConsoleDriver" /> (one of the implementations UnixMainLoop,
    ///     NetMainLoop or WindowsMainLoop).
    /// </param>
    /// <param name="consoleKeyReaderFn"></param>
    //public WebMainLoopDriver(Func<ConsoleKeyInfo>? consoleKeyReaderFn = null)
    //{
    //    this._consoleKeyReaderFn =
    //        consoleKeyReaderFn ?? throw new ArgumentNullException(paramName: nameof(consoleKeyReaderFn));
    //}
    public WebMainLoopDriver(WebConsole webConsole)
    {
        this._webConsole = webConsole ?? throw new ArgumentNullException("Console driver instance must be provided.");
        this._webConsole.ReadConsoleInput += _webConsole_ReadConsoleInput;
    }

    private void _webConsole_ReadConsoleInput(InputResult obj)
    {
        _inputResult.Enqueue(obj);
        _keyReady.Set();
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
        _keyReady.Set();
    }

    bool IMainLoopDriver.EventsPending(bool wait)
    {
        if (CheckTimers(wait, out var waitTimeout))
        {
            return true;
        }

        try
        {
            if (!tokenSource.IsCancellationRequested)
            {
                _keyReady.Wait(waitTimeout, tokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            return true;
        }
        finally
        {
            _keyReady.Reset();
        }

        if (!tokenSource.IsCancellationRequested)
        {
            return _inputResult.Count > 0 || CheckTimers(wait, out _);
        }

        tokenSource.Dispose();
        tokenSource = new CancellationTokenSource();
        return true;
    }

    bool CheckTimers(bool wait, out int waitTimeout)
    {
        long now = DateTime.UtcNow.Ticks;

        if (_mainLoop.Timeouts.Count > 0)
        {
            waitTimeout = (int)((_mainLoop.Timeouts.Keys[0] - now) / TimeSpan.TicksPerMillisecond);
            if (waitTimeout < 0)
                return true;
        }
        else
        {
            waitTimeout = -1;
        }

        if (!wait)
            waitTimeout = 0;

        int ic;
        lock (_mainLoop.IdleHandlers)
        {
            ic = _mainLoop.IdleHandlers.Count;
        }

        return ic > 0;
    }

    void IMainLoopDriver.MainIteration()
    {
        while (_inputResult.Count > 0)
        {
            ProcessInput?.Invoke(obj: _inputResult.Dequeue().Value);
        }
    }
}