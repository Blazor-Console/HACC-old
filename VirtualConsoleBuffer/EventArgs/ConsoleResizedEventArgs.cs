namespace HACC.VirtualConsoleBuffer.EventArgs;

public record ConsoleResizedEventArgs : VirtualConsoleEventArgs
{
    public readonly int NewHeight;
    public readonly int NewWidth;
    public readonly int OldHeight;
    public readonly int OldWidth;

    public ConsoleResizedEventArgs(VirtualConsoleBuffer sender, int oldWidth, int oldHeight, int newWidth,
        int newHeight) : base(sender: sender)
    {
        OldWidth = oldWidth;
        OldHeight = oldHeight;
        NewWidth = newWidth;
        NewHeight = newHeight;
    }
}