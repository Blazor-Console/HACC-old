namespace HACC.VirtualConsoleBuffer.EventArgs
{
    using System.Drawing;

    public record NewFrameEventArgs<T> : VirtualConsoleEventArgs<T>
    {
        public readonly Bitmap? OldFrame;
        public readonly Bitmap NewFrame;
        public NewFrameEventArgs(VirtualConsoleBuffer<T> sender, Bitmap? oldFrame, Bitmap newFrame) : base(sender: sender)
        {
            OldFrame = oldFrame;
            NewFrame = newFrame;
        }
    }
}