using HACC.Spectre;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;
using Terminal.Gui;

namespace HACC.Models.Driver;

//
// Summary:
//     Represents the standard input, output, and error streams for console applications.
//     This class cannot be inherited.
public partial class Html5AnsiConsoleCanvas : ConsoleDriver, IAnsiConsole
{
    private readonly ILogger Logger;

    private TerminalSettings TerminalSettings;

    public Html5AnsiConsoleCanvas(ILogger logger, TerminalSettings? terminalSettings = null)
    {
        this.Logger = logger;
        this.TerminalSettings = terminalSettings.HasValue ? terminalSettings.Value : new TerminalSettings();
        this.InternalCharacterBuffer = new CharacterBuffer(
            logger: logger,
            columns: this.TerminalSettings.Columns,
            rows: this.TerminalSettings.Rows);
    }

    public Profile Profile => throw new NotImplementedException();

    public IExclusivityMode ExclusivityMode => new DefaultExclusivityMode();

    public RenderPipeline Pipeline => throw new NotImplementedException();
}