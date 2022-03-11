using HACC.Models.Driver;
using HACC.Spectre;

namespace HACC.Models.EventArgs;

public record ConsoleResizedEventArgs : VirtualConsoleEventArgs
{
    public readonly int NewHeight;
    public readonly int NewWidth;
    public readonly int OldHeight;
    public readonly int OldWidth;

    public ConsoleResizedEventArgs(Html5AnsiConsoleCanvas sender, int oldWidth, int oldHeight, int newWidth,
        int newHeight) : base(sender: sender)
    {
        OldWidth = oldWidth;
        OldHeight = oldHeight;
        NewWidth = newWidth;
        NewHeight = newHeight;
    }
}