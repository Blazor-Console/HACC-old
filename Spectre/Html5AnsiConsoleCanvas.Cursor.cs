using System.Drawing;
using Spectre.Console;

namespace HACC.Spectre;

//
// Summary:
//     Represents the standard input, output, and error streams for console applications.
//     This class cannot be inherited.
public partial class Html5AnsiConsoleCanvas : IAnsiConsole
{
    //
    // Summary:
    //     Gets or sets the column position of the cursor within the buffer area.
    //
    // Returns:
    //     The current position, in columns, of the cursor.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The value in a set operation is less than zero. -or- The value in a set operation
    //     is greater than or equal to System.Console.BufferWidth.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public int CursorLeft
    {
        get => TerminalSettings.CursorPosition.X;
        set
        {
            TerminalSettings.CursorPosition = new Point(
                x: value,
                y: TerminalSettings.CursorPosition.Y);

            throw new NotImplementedException();
        }
    }

    //
    // Summary:
    //     Gets or sets the height of the cursor within a character cell.
    //
    // Returns:
    //     The size of the cursor expressed as a percentage of the height of a character
    //     cell. The property value ranges from 1 to 100.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The value specified in a set operation is less than 1 or greater than 100.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.PlatformNotSupportedException:
    //     The set operation is invoked on an operating system other than Windows.
    public int CursorSize
    {
        get => TerminalSettings.CursorSize;
        set
        {
            TerminalSettings.CursorSize = value;

            throw new NotImplementedException();
        }
    }

    //
    // Summary:
    //     Gets or sets the row position of the cursor within the buffer area.
    //
    // Returns:
    //     The current position, in rows, of the cursor.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The value in a set operation is less than zero. -or- The value in a set operation
    //     is greater than or equal to System.Console.BufferHeight.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public int CursorTop
    {
        get => TerminalSettings.CursorPosition.Y;
        set
        {
            TerminalSettings.CursorPosition = new Point(
                x: TerminalSettings.CursorPosition.X,
                y: value);

            throw new NotImplementedException();
        }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether the cursor is visible.
    //
    // Returns:
    //     true if the cursor is visible; otherwise, false.
    //
    // Exceptions:
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.PlatformNotSupportedException:
    //     The get operation is invoked on an operating system other than Windows.
    public bool CursorVisible
    {
        get => TerminalSettings.CursorVisible;
        set
        {
            TerminalSettings.CursorVisible = value;

            throw new NotImplementedException();
        }
    }


    public IAnsiConsoleCursor Cursor => throw new NotImplementedException();

    //
    // Summary:
    //     Gets the position of the cursor.
    //
    // Returns:
    //     The column and row position of the cursor.
    public (int Left, int Top) GetCursorPosition()
    {
        return (
            Left: TerminalSettings.CursorPosition.X,
            Top: TerminalSettings.CursorPosition.Y
        );
    }

    //
    // Summary:
    //     Sets the position of the cursor.
    //
    // Parameters:
    //   left:
    //     The column position of the cursor. Columns are numbered from left to right starting
    //     at 0.
    //
    //   top:
    //     The row position of the cursor. Rows are numbered from top to bottom starting
    //     at 0.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     left or top is less than zero. -or- left is greater than or equal to System.Console.BufferWidth.
    //     -or- top is greater than or equal to System.Console.BufferHeight.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void SetCursorPosition(int left, int top)
    {
        TerminalSettings.SetCursorPosition(x: left, y: top);
    }
}