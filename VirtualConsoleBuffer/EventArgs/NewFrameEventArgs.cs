namespace HACC.VirtualConsoleBuffer.EventArgs
{
    using System.Drawing;

    public record NewFrameEventArgs : VirtualConsoleEventArgs
    {
        public readonly Bitmap? OldFrame;
        public readonly Bitmap NewFrame;
        public NewFrameEventArgs(VirtualConsoleBuffer sender, Bitmap? oldFrame, Bitmap newFrame) : base(sender: sender)
        {
            OldFrame = oldFrame;
            NewFrame = newFrame;
        }
    }
}