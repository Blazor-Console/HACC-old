namespace HACC.VirtualConsoleBuffer;

public static class Defaults
{
    public const int MaximumColumns = 1000;
    public const int MaximumRows = 1000;
    public const int InitialTerminalWidth = 640;
    public const int InitialTerminalHeight = 480;
    public const int InitialColumns = 80;
    public const int InitialRows = 24;
    public const bool CursorVisibility = true;
    public const bool StatusVisibility = true;
    public const bool TitleVisibility = true;
    public const ConsoleColor BackgroundColor = ConsoleColor.Black;
    public const ConsoleColor ForegroundColor = ConsoleColor.White;
    public const CursorType CursorShape = CursorType.Block;
}