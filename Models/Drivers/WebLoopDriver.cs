using Terminal.Gui;

namespace HACC.Models.Drivers;

/// <summary>
///     WebMainloop intended to be used with .NET Blazor
/// </summary>
/// <remarks>
///     This implementation is used for WebLoop.
/// </remarks>
public class WebLoopDriver : IMainLoopDriver
{
    // ReSharper disable once NotAccessedField.Local
    private readonly Func<ConsoleKeyInfo> _consoleKeyReaderFn;

    // ReSharper disable once HeapView.ObjectAllocation.Evident
    private readonly AutoResetEvent _keyReady = new(initialState: false);

    // ReSharper disable once HeapView.ObjectAllocation.Evident
    private readonly AutoResetEvent _waitForProbe = new(initialState: false);
    private ConsoleKeyInfo? _keyResult;
    private WebMainLoop _mainLoop;

    /// <summary>
    ///     Invoked when a Key is pressed.
    /// </summary>
    public Action<ConsoleKeyInfo> KeyPressed;

    /// <summary>
    ///     Initializes the class.
    /// </summary>
    /// <remarks>
    ///     Passing a consoleKeyReaderfn is provided to support unit test scenarios.
    /// </remarks>
    /// <param name="mainLoop"></param>
    /// <param name="keyPressed"></param>
    /// <param name="consoleKeyReaderFn">The method to be called to get a key from the console.</param>
    public WebLoopDriver(WebMainLoop mainLoop, Action<ConsoleKeyInfo> keyPressed,
        Func<ConsoleKeyInfo>? consoleKeyReaderFn = null)
    {
        this._mainLoop = mainLoop;
        this.KeyPressed = keyPressed;
        this._consoleKeyReaderFn =
            consoleKeyReaderFn ?? throw new ArgumentNullException(paramName: nameof(consoleKeyReaderFn));
    }


    void IMainLoopDriver.Setup(MainLoop mainLoop)
    {
        this._mainLoop = (WebMainLoop) mainLoop;
    }

    void IMainLoopDriver.Wakeup()
    {
    }

    bool IMainLoopDriver.EventsPending(bool wait)
    {
        var now = DateTime.UtcNow.Ticks;

        int waitTimeout;
        var timeouts = this._mainLoop.Timeouts;
        if (timeouts.Count > 0)
        {
            waitTimeout = (int) ((timeouts.Keys[index: 0] - now) / TimeSpan.TicksPerMillisecond);
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
        this.KeyPressed.Invoke(obj: this._keyResult.Value);
        this._keyResult = null;
    }
}