using HACC.Components;
using HACC.Spectre;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;
using Terminal.Gui;
using Console = HACC.Components.Console;

namespace HACC.Models.Drivers;

//
// Summary:
//     Represents the standard input, output, and error streams for console applications.
//     This class cannot be inherited.
public partial class CanvasConsole : ConsoleDriver, IAnsiConsole
{
    private readonly Console _console;
    private readonly ILogger _logger;

    private TerminalSettings _terminalSettings;

    /// <summary>
    /// </summary>
    /// <param name="logger">dependency injected logger</param>
    /// <param name="console">dependency injected console</param>
    /// <param name="webClipboard">dependency injected webClipboard</param>
    /// <param name="terminalSettings"></param>
    public CanvasConsole(ILogger logger, Console console, WebClipboard webClipboard,
        TerminalSettings? terminalSettings = null)
    {
        this._console = console;
        this._logger = logger;
        this.Clipboard = webClipboard;
        this._terminalSettings = terminalSettings ?? new TerminalSettings();
        this._internalCharacterBuffer = new CharacterBuffer(
            logger: logger,
            columns: this._terminalSettings.WindowColumns,
            rows: this._terminalSettings.WindowRows);
    }

    public Profile Profile => throw new NotImplementedException();

    public IExclusivityMode ExclusivityMode => new DefaultExclusivityMode();

    public RenderPipeline Pipeline => throw new NotImplementedException();
}