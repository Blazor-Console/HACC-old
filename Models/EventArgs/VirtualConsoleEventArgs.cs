using HACC.Models.Drivers;
using HACC.Spectre;

namespace HACC.Models.EventArgs;

public record VirtualConsoleEventArgs
{
    public readonly Html5AnsiConsoleCanvas Console;

    public VirtualConsoleEventArgs(Html5AnsiConsoleCanvas sender)
    {
        Console = sender;
    }
}