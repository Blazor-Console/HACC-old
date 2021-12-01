using HACC.Models;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace HACC.Spectre;

//
// Summary:
//     Represents the standard input, output, and error streams for console applications.
//     This class cannot be inherited.
public partial class Html5AnsiConsoleCanvas : IAnsiConsole
{
    private readonly ILogger Logger;

    private TerminalSettings TerminalSettings;

    public Html5AnsiConsoleCanvas(ILogger logger, TerminalSettings? terminalSettings = null)
    {
        Logger = logger;
        TerminalSettings = terminalSettings.HasValue ? terminalSettings.Value : new TerminalSettings();
        InternalCharacterBuffer = new CharacterBuffer(
            logger: logger,
            columns: TerminalSettings.Columns,
            rows: TerminalSettings.Rows);
    }

    public Profile Profile => throw new NotImplementedException();

    public IExclusivityMode ExclusivityMode => new DefaultExclusivityMode();

    public RenderPipeline Pipeline => throw new NotImplementedException();
}