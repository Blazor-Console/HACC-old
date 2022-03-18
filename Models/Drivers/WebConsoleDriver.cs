﻿using HACC.Components;
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
public sealed partial class WebConsoleDriver : ConsoleDriver, IAnsiConsole
{
    private readonly WebConsole _console;
    private readonly ILogger _logger;

    /// <summary>
    ///     Initializes a web console driver.
    /// </summary>
    /// <param name="logger">dependency injected logger</param>
    public WebConsoleDriver(ILogger logger)
    {
        this._logger = logger;
        this.Clipboard = new WebClipboard();
        // ReSharper disable HeapView.ObjectAllocation.Evident
        this.TerminalSettings = new TerminalSettings();
        this.Contents = new int[this.BufferRows, this.BufferColumns, 3];
        this._dirtyLine = new bool [this.BufferRows];
        this._console = new WebConsole(logger: logger,
            webConsoleDriver: this);
        // ReSharper restore HeapView.ObjectAllocation.Evident
    }

    // TODO: resize, etc if terminal settings updated
    public TerminalSettings TerminalSettings { get; private set; }

    public Profile Profile => throw new NotImplementedException();

    public IExclusivityMode ExclusivityMode => new DefaultExclusivityMode();

    public RenderPipeline Pipeline => throw new NotImplementedException();
}