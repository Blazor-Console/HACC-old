using System.Runtime.Versioning;
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
[SupportedOSPlatform(platformName: "browser")]
public partial class WebConsole : ConsoleDriver, IAnsiConsole
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
    public WebConsole(ILogger logger, Console console, WebClipboard webClipboard,
        TerminalSettings? terminalSettings = null)
    {
        this._console = console;
        this._logger = logger;
        this.Clipboard = webClipboard;
        this._terminalSettings = terminalSettings ?? new TerminalSettings();
    }

    public Profile Profile => throw new NotImplementedException();

    public IExclusivityMode ExclusivityMode => new DefaultExclusivityMode();

    public RenderPipeline Pipeline => throw new NotImplementedException();
}