using HACC.Models.Drivers;

namespace HACC.Models.EventArgs;

public record VirtualConsoleEventArgs
{
    public readonly Html5AnsiConsoleCanvas Console;

    public VirtualConsoleEventArgs(Html5AnsiConsoleCanvas sender)
    {
        this.Console = sender;
    }
}