namespace HACC.VirtualConsoleBuffer.EventArgs
{
    public record ConsoleResizedEventArgs<T> : VirtualConsoleEventArgs<T>
    {
        public readonly int OldWidth;
        public readonly int OldHeight;
        public readonly int NewWidth;
        public readonly int NewHeight;

        public ConsoleResizedEventArgs(VirtualConsoleBuffer.VirtualConsoleBuffer<T> sender, int oldWidth, int oldHeight, int newWidth, int newHeight) : base(sender: sender)
        {
            OldWidth = oldWidth;
            OldHeight = oldHeight;
            NewWidth = newWidth;
            NewHeight = newHeight;
        }
    }
}