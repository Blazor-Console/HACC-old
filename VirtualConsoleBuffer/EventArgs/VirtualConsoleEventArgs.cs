namespace HACC.VirtualConsoleBuffer.EventArgs
{
    public record VirtualConsoleEventArgs<T>
    {
        public readonly VirtualConsoleBuffer<T> Console;
        public VirtualConsoleEventArgs(VirtualConsoleBuffer<T> sender)
        {
            Console = sender;
        }
    }
}