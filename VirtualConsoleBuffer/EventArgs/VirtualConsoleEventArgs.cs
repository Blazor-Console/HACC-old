namespace HACC.VirtualConsoleBuffer.EventArgs
{
    public record VirtualConsoleEventArgs
    {
        public readonly VirtualConsoleBuffer Console;
        public VirtualConsoleEventArgs(VirtualConsoleBuffer sender)
        {
            Console = sender;
        }
    }
}