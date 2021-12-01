using System.Drawing;
using HACC.Spectre;

namespace HACC.Models.EventArgs;

public record NewFrameEventArgs : VirtualConsoleEventArgs
{
    public readonly Bitmap NewFrame;
    public readonly Bitmap? OldFrame;

    public NewFrameEventArgs(Html5AnsiConsoleCanvas sender, Bitmap? oldFrame, Bitmap newFrame) : base(sender: sender)
    {
        OldFrame = oldFrame;
        NewFrame = newFrame;
    }
}