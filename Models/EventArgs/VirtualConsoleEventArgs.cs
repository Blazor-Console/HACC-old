using HACC.Models.Drivers;

namespace HACC.Models.EventArgs;

public record VirtualConsoleEventArgs
{
    public readonly WebConsole Console;

    public VirtualConsoleEventArgs(WebConsole sender)
    {
        this.Console = sender;
    }
}