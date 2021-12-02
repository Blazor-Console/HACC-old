using System.Drawing;

namespace HACC.VirtualConsoleBuffer;

public struct TerminalSettings
{
    /// <summary>
    ///     Window/Terminal title
    /// </summary>
    public string Title;

    /// <summary>
    ///     Terminal window width in pixels
    /// </summary>
    public int TerminalWidth;

    /// <summary>
    ///     Terminal window height in pixels
    /// </summary>
    public int TerminalHeight;

    /// <summary>
    ///     Terminal window width in characters
    /// </summary>
    public int Columns;

    /// <summary>
    ///     Terminal window height in characters
    /// </summary>
    public int Rows;

    /// <summary>
    ///     Whether cursor is visible
    /// </summary>
    public bool CursorVisible;

    /// <summary>
    ///     Whether the status bar is visible
    /// </summary>
    public bool StatusVisible;

    /// <summary>
    ///     Whether the title bar is visible
    /// </summary>
    public bool TitleVisible;

    /// <summary>
    ///     Cursor position in characters
    /// </summary>
    public Point CursorPosition;

    /// <summary>
    ///     Cursor display shape/type
    /// </summary>
    public CursorType CursorType;

    /// <summary>
    ///     Cursor height in percentage of character
    /// </summary>
    public int CursorHeight;

    /// <summary>
    ///     Summary:
    ///     Gets or sets the height of the cursor within a character cell.
    ///     Returns:
    ///     The size of the cursor expressed as a percentage of the height of a character
    ///     cell. The property value ranges from 1 to 100.
    /// </summary>
    public int CursorSize;

    /// <summary>
    ///     Terminal default background color
    /// </summary>
    public ConsoleColor TerminalBackground;

    /// <summary>
    ///     Terminal default foreground color
    /// </summary>
    public ConsoleColor TerminalForeground;

    public TerminalSettings()
    {
        Title = "";
        TerminalWidth = Defaults.InitialTerminalWidth;
        TerminalHeight = Defaults.InitialTerminalHeight;
        Columns = Defaults.InitialColumns;
        Rows = Defaults.InitialRows;
        CursorVisible = Defaults.CursorVisibility;
        StatusVisible = Defaults.StatusVisibility;
        TitleVisible = Defaults.TitleVisibility;
        CursorPosition = new Point(
            0,
            0);
        CursorHeight = 100;
        CursorSize = 100;
        TerminalBackground = Defaults.BackgroundColor;
        TerminalForeground = Defaults.ForegroundColor;
        CursorType = Defaults.CursorShape;
    }
}