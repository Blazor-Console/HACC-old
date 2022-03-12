using System.Drawing;
using HACC.Configuration;
using HACC.Enumerations;

namespace HACC.Models;

public record TerminalSettings
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
    public int BufferColumns;

    /// <summary>
    ///     Terminal window height in characters
    /// </summary>
    public int BufferRows;
    
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

    public TerminalSettings(
        string title = "",
        int terminalWidth = Defaults.InitialTerminalWidth,
        int terminalHeight = Defaults.InitialTerminalHeight,
        int bufferColumns = Defaults.InitialBufferColumns,
        int bufferRows = Defaults.InitialBufferRows,
        int columns = Defaults.InitialColumns,
        int rows = Defaults.InitialRows,
        bool cursorVisible = Defaults.CursorVisibility,
        bool statusVisible = Defaults.StatusVisibility,
        bool titleVisible = Defaults.TitleVisibility,
        Point? cursorPosition = null,
        CursorType cursorType = Defaults.CursorShape,
        int cursorHeight = Defaults.CursorHeight,
        int cursorSize = Defaults.CursorSize,
        ConsoleColor terminalBackground = Defaults.BackgroundColor,
        ConsoleColor terminalForeground = Defaults.ForegroundColor)
    {
        this.Title = title;
        this.TerminalWidth = terminalWidth;
        this.TerminalHeight = terminalHeight;
        this.BufferColumns = bufferColumns;
        this.BufferRows = bufferRows;
        this.Columns = columns;
        this.Rows = rows;
        this.CursorVisible = cursorVisible;
        this.StatusVisible = statusVisible;
        this.TitleVisible = titleVisible;
        this.CursorPosition = cursorPosition ?? new Point(
            x: 0,
            y: 0);
        this.CursorType = cursorType;
        this.CursorHeight = cursorHeight;
        this.CursorSize = cursorSize;
        this.TerminalBackground = terminalBackground;
        this.TerminalForeground = terminalForeground;
    }

    public void SetCursorPosition(int x, int y)
    {
        CursorPosition = new Point(x: x, y: y);
    }
}