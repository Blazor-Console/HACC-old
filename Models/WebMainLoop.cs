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
public class WebMainLoop : MainLoop
{
    private List<Func<bool>> _idleHandlers = new();

    private SortedList<long, Timeout> _timeouts = new();

    /// <summary>
    ///     Creates a new Mainloop.
    /// </summary>
    /// <param name="driver">
    ///     Should match the <see cref="ConsoleDriver" /> (one of the implementations UnixMainLoop,
    ///     NetMainLoop or WindowsMainLoop).
    /// </param>
    public WebMainLoop(IMainLoopDriver driver) : base(driver: driver)
    {
    }

    public SortedList<long, Timeout> Timeouts
    {
        get
        {
            lock (this._timeouts)
            {
                return this._timeouts;
            }
        }
    }

    private void AddTimeout(TimeSpan time, Timeout timeout)
    {
        lock (this._timeouts)
        {
            var k = (DateTime.UtcNow + time).Ticks;
            while (this._timeouts.ContainsKey(key: k))
            {
                k = (DateTime.UtcNow + time).Ticks;
            }

            this._timeouts.Add(key: k, value: timeout);
        }
    }

    /// <summary>
    ///     Adds a timeout to the mainloop.
    /// </summary>
    /// <remarks>
    ///     When time specified passes, the callback will be invoked.
    ///     If the callback returns true, the timeout will be reset, repeating
    ///     the invocation. If it returns false, the timeout will stop and be removed.
    ///     The returned value is a token that can be used to stop the timeout
    ///     by calling <see cref="RemoveTimeout(object)" />.
    /// </remarks>
    public new object AddTimeout(TimeSpan time, Func<MainLoop, bool> callback)
    {
        if (callback == null)
            throw new ArgumentNullException(paramName: nameof(callback));
        var timeout = new Timeout(span: time, callback: callback);
        this.AddTimeout(time: time, timeout: timeout);
        return timeout;
    }

    /// <summary>
    ///     Removes a previously scheduled timeout
    /// </summary>
    /// <remarks>
    ///     The token parameter is the value returned by AddTimeout.
    /// </remarks>
    /// Returns
    /// <c>true</c>
    /// if the timeout is successfully removed; otherwise,
    /// <c>false</c>
    /// .
    /// This method also returns
    /// <c>false</c>
    /// if the timeout is not found.
    public new bool RemoveTimeout(object token)
    {
        lock (this._timeouts)
        {
            var idx = this._timeouts.IndexOfValue(value: (token as Timeout)!);
            if (idx == -1)
                return false;
            this._timeouts.RemoveAt(index: idx);
        }

        return true;
    }

    private void RunTimers()
    {
        var now = DateTime.UtcNow.Ticks;
        lock (this._timeouts)
        {
            var copy = this._timeouts;
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            this._timeouts = new SortedList<long, Timeout>();
            // ReSharper disable once HeapView.ObjectAllocation.Possible
            foreach (var (k, timeout) in copy)
            {
                if (k < now)
                {
                    if (timeout.Callback(arg: this)) this.AddTimeout(time: timeout.Span, timeout: timeout);
                }
                else
                {
                    lock (this._timeouts)
                    {
                        this._timeouts.Add(key: k, value: timeout);
                    }
                }
            }
        }
    }

    private void RunIdle()
    {
        List<Func<bool>> iterate;
        lock (this._idleHandlers)
        {
            iterate = this._idleHandlers;
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            this._idleHandlers = new List<Func<bool>>();
        }

        // ReSharper disable once HeapView.ObjectAllocation
        // ReSharper disable once HeapView.ObjectAllocation.Possible
        foreach (var idle in iterate.Where(predicate: idle => idle()))
        {
            lock (this._idleHandlers)
            {
                this._idleHandlers.Add(item: idle);
            }
        }
    }


    /// <summary>
    ///     Runs one iteration of timers and file watches
    /// </summary>
    /// <remarks>
    ///     You use this to process all pending events (timers, idle handlers and file watches).
    ///     You can use it like this:
    ///     while (main.EvensPending ()) MainIteration ();
    /// </remarks>
    public new void MainIteration()
    {
        lock (this._timeouts)
        {
            if (this._timeouts.Count > 0) this.RunTimers();
        }

        this.Driver.MainIteration();

        lock (this._idleHandlers)
        {
            if (this._idleHandlers.Count > 0) this.RunIdle();
        }
    }

    public record Timeout
    {
        public readonly Func<MainLoop, bool> Callback = null!;
        public readonly TimeSpan Span;

        public Timeout(TimeSpan span, Func<MainLoop, bool> callback)
        {
            this.Span = span;
            this.Callback = callback;
        }
    }
}