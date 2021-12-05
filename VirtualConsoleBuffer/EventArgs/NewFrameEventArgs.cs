using System.Drawing;

namespace HACC.VirtualConsoleBuffer.EventArgs;

public record NewFrameEventArgs : VirtualConsoleEventArgs
{
    public readonly Bitmap NewFrame;
    public readonly Bitmap? OldFrame;

    public NewFrameEventArgs(VirtualConsoleBuffer sender, Bitmap? oldFrame, Bitmap newFrame) : base(sender: sender)
    {
        OldFrame = oldFrame;
        NewFrame = newFrame;
    }
}