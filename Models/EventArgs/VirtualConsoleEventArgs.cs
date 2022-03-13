using HACC.Models.Drivers;

namespace HACC.Models.EventArgs;

public record VirtualConsoleEventArgs
{
    public readonly CanvasConsole Console;

    public VirtualConsoleEventArgs(CanvasConsole sender)
    {
        this.Console = sender;
    }
}