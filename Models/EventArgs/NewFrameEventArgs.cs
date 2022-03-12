using System.Drawing;
using HACC.Models.Drivers;

namespace HACC.Models.EventArgs;

public record NewFrameEventArgs : VirtualConsoleEventArgs
{
    public readonly Bitmap NewFrame;
    public readonly Bitmap? OldFrame;

    public NewFrameEventArgs(Html5AnsiConsoleCanvas sender, Bitmap? oldFrame, Bitmap newFrame) : base(sender: sender)
    {
        this.OldFrame = oldFrame;
        this.NewFrame = newFrame;
    }
}