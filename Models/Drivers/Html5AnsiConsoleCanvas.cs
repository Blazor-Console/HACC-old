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
public partial class Html5AnsiConsoleCanvas : ConsoleDriver, IAnsiConsole
{
    private readonly Console _console;
    private readonly ILogger _logger;

    private TerminalSettings _terminalSettings;


    public Html5AnsiConsoleCanvas(ILogger logger, Console console, TerminalSettings? terminalSettings = null)
    {
        this._console = console;
        this._logger = logger;
        this._terminalSettings = terminalSettings ?? new TerminalSettings();
        this._internalCharacterBuffer = new CharacterBuffer(
            logger: logger,
            columns: this._terminalSettings.Columns,
            rows: this._terminalSettings.Rows);
    }

    public Profile Profile => throw new NotImplementedException();

    public IExclusivityMode ExclusivityMode => new DefaultExclusivityMode();

    public RenderPipeline Pipeline => throw new NotImplementedException();
}