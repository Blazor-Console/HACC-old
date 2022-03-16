using System.Runtime.Versioning;
using HACC.Components;
using HACC.Spectre;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;
using Terminal.Gui;

namespace HACC.Models.Drivers;

/// <summary>
///     Represents the standard input, output, and error streams for console applications.
///     This class cannot be inherited.
/// </summary>
[SupportedOSPlatform(platformName: "browser")]
public sealed partial class WebConsoleDriver : ConsoleDriver, IAnsiConsole
{
    private readonly WebConsole _console;
    private readonly ILogger _logger;

    private TerminalSettings _terminalSettings;

    /// <summary>
    /// </summary>
    /// <param name="logger">dependency injected logger</param>
    /// <param name="console">dependency injected console</param>
    /// <param name="webClipboard">dependency injected webClipboard</param>
    /// <param name="terminalSettings"></param>
    public WebConsoleDriver(ILogger logger, WebClipboard webClipboard, WebConsole console,
        TerminalSettings? terminalSettings = null)
    {
        this._console = console;
        this._logger = logger;
        this.Clipboard = webClipboard;
        // ReSharper disable HeapView.ObjectAllocation.Evident
        this._terminalSettings = terminalSettings ?? new TerminalSettings();
        this.Contents = new int[this.BufferRows, this.BufferColumns, 3];
        this._dirtyLine = new bool [this.BufferRows];
        // ReSharper restore HeapView.ObjectAllocation.Evident
    }

    public Profile Profile => throw new NotImplementedException();

    public IExclusivityMode ExclusivityMode => new DefaultExclusivityMode();

    public RenderPipeline Pipeline => throw new NotImplementedException();
}