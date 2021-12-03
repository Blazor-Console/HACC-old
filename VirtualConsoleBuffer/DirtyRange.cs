namespace HACC.VirtualConsoleBuffer;

public struct DirtyRange
{
    public readonly int XStart;
    public readonly int XEnd;
    public readonly int Y;

    public DirtyRange(int xStart, int xEnd, int y)
    {
        this.XStart = xStart;
        this.XEnd = xEnd;
        this.Y = y;
    }
}