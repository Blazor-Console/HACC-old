﻿using HACC.Configuration;
using HACC.Models.EventArgs;
using Spectre.Console;

namespace HACC.Models.Drivers;

//
// Summary:
//     Represents the standard input, output, and error streams for console applications.
//     This class cannot be inherited.
public partial class Html5AnsiConsoleCanvas
{
    public delegate void NewFrameHandler(object sender, NewFrameEventArgs e);

    private readonly CharacterBuffer _internalCharacterBuffer;

    //
    // Summary:
    //     Gets or sets the height of the buffer area.
    //
    // Returns:
    //     The current height, in rows, of the buffer area.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The value in a set operation is less than or equal to zero. -or- The value in
    //     a set operation is greater than or equal to System.Int16.MaxValue. -or- The value
    //     in a set operation is less than System.Console.WindowTop + System.Console.WindowHeight.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.PlatformNotSupportedException:
    //     The set operation is invoked on an operating system other than Windows.
    public int BufferHeight
    {
        get => this._terminalSettings.Rows;
        set
        {
            this._terminalSettings.Rows = value;
            this.TerminalResized?.Invoke();
        }
    }

    //
    // Summary:
    //     Gets or sets the width of the buffer area.
    //
    // Returns:
    //     The current width, in columns, of the buffer area.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The value in a set operation is less than or equal to zero. -or- The value in
    //     a set operation is greater than or equal to System.Int16.MaxValue. -or- The value
    //     in a set operation is less than System.Console.WindowLeft + System.Console.WindowWidth.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.PlatformNotSupportedException:
    //     The set operation is invoked on an operating system other than Windows.
    public int BufferWidth
    {
        get => this._terminalSettings.Columns;
        set
        {
            this._terminalSettings.Rows = value;
            this.TerminalResized?.Invoke();
        }
    }

    //
    // Summary:
    //     Gets the largest possible number of console window rows, based on the current
    //     font and screen resolution.
    //
    // Returns:
    //     The height of the largest possible console window measured in rows.
    public int LargestWindowHeight => Defaults.MaximumRows;

    //
    // Summary:
    //     Gets the largest possible number of console window columns, based on the current
    //     font and screen resolution.
    //
    // Returns:
    //     The width of the largest possible console window measured in columns.
    public int LargestWindowWidth => Defaults.MaximumColumns;

    //
    // Summary:
    //     Gets or sets the title to display in the console title bar.
    //
    // Returns:
    //     The string to be displayed in the title bar of the console. The maximum length
    //     of the title string is 24500 characters.
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     In a get operation, the retrieved title is longer than 24500 characters.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     In a set operation, the specified title is longer than 24500 characters.
    //
    //   T:System.ArgumentNullException:
    //     In a set operation, the specified title is null.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.PlatformNotSupportedException:
    //     The get operation is invoked on an operating system other than Windows.
    public string Title
    {
        get => this._terminalSettings.Title;
        set
        {
            this._terminalSettings.Title = value;

            throw new NotImplementedException();
        }
    }

    //
    // Summary:
    //     Gets or sets the height of the console window area.
    //
    // Returns:
    //     The height of the console window measured in rows.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The value of the System.Console.WindowWidth property or the value of the System.Console.WindowHeight
    //     property is less than or equal to 0. -or- The value of the System.Console.WindowHeight
    //     property plus the value of the System.Console.WindowTop property is greater than
    //     or equal to System.Int16.MaxValue. -or- The value of the System.Console.WindowWidth
    //     property or the value of the System.Console.WindowHeight property is greater
    //     than the largest possible window width or height for the current screen resolution
    //     and console font.
    //
    //   T:System.IO.IOException:
    //     Error reading or writing information.
    //
    //   T:System.PlatformNotSupportedException:
    //     The set operation is invoked on an operating system other than Windows.
    public int WindowRows
    {
        get => this._terminalSettings.Rows;
        set
        {
            if (value > LargestWindowHeight)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The value of the WindowRows property is greater than the largest possible window height");
            }
            this._terminalSettings.Rows = value;
        }
    }

    //
    // Summary:
    //     Gets or sets the leftmost position of the console window area relative to the
    //     screen buffer.
    //
    // Returns:
    //     The leftmost console window position measured in columns.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     In a set operation, the value to be assigned is less than zero. -or- As a result
    //     of the assignment, System.Console.WindowLeft plus System.Console.WindowWidth
    //     would exceed System.Console.BufferWidth.
    //
    //   T:System.IO.IOException:
    //     Error reading or writing information.
    //
    //   T:System.PlatformNotSupportedException:
    //     The set operation is invoked on an operating system other than Windows.
    public int WindowLeft
    {
        get => 0;
        set { }
    }

    //
    // Summary:
    //     Gets or sets the top position of the console window area relative to the screen
    //     buffer.
    //
    // Returns:
    //     The uppermost console window position measured in rows.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     In a set operation, the value to be assigned is less than zero. -or- As a result
    //     of the assignment, System.Console.WindowTop plus System.Console.WindowHeight
    //     would exceed System.Console.BufferHeight.
    //
    //   T:System.IO.IOException:
    //     Error reading or writing information.
    //
    //   T:System.PlatformNotSupportedException:
    //     The set operation is invoked on an operating system other than Windows.
    public int WindowTop
    {
        get => 0;
        set { }
    }

    //
    // Summary:
    //     Gets or sets the width of the console window.
    //
    // Returns:
    //     The width of the console window measured in columns.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     The value of the System.Console.WindowWidth property or the value of the System.Console.WindowHeight
    //     property is less than or equal to 0. -or- The value of the System.Console.WindowHeight
    //     property plus the value of the System.Console.WindowTop property is greater than
    //     or equal to System.Int16.MaxValue. -or- The value of the System.Console.WindowWidth
    //     property or the value of the System.Console.WindowHeight property is greater
    //     than the largest possible window width or height for the current screen resolution
    //     and console font.
    //
    //   T:System.IO.IOException:
    //     Error reading or writing information.
    //
    //   T:System.PlatformNotSupportedException:
    //     The set operation is invoked on an operating system other than Windows.
    public int WindowColumns
    {
        get => this._terminalSettings.Columns;
        set
        {
            if (value > LargestWindowWidth)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "The value of the WindowColumns property is greater than the largest possible window width");
            }
            this._terminalSettings.Columns = value;
        }
    }

    public void Clear(bool home)
    {
        throw new NotImplementedException();
    }

    public event NewFrameHandler NewFrame;

    //
    // Parameters:
    //   sourceLeft:
    //     The leftmost column of the source area.
    //
    //   sourceTop:
    //     The topmost row of the source area.
    //
    //   sourceWidth:
    //     The number of columns in the source area.
    //
    //   sourceHeight:
    //     The number of rows in the source area.
    //
    //   targetLeft:
    //     The leftmost column of the destination area.
    //
    //   targetTop:
    //     The topmost row of the destination area.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     One or more of the parameters is less than zero. -or- sourceLeft or targetLeft
    //     is greater than or equal to System.Console.BufferWidth. -or- sourceTop or targetTop
    //     is greater than or equal to System.Console.BufferHeight. -or- sourceTop + sourceHeight
    //     is greater than or equal to System.Console.BufferHeight. -or- sourceLeft + sourceWidth
    //     is greater than or equal to System.Console.BufferWidth.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.PlatformNotSupportedException:
    //     The current operating system is not Windows.
    public void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft,
        int targetTop)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Copies a specified source area of the screen buffer to a specified destination
    //     area.
    //
    // Parameters:
    //   sourceLeft:
    //     The leftmost column of the source area.
    //
    //   sourceTop:
    //     The topmost row of the source area.
    //
    //   sourceWidth:
    //     The number of columns in the source area.
    //
    //   sourceHeight:
    //     The number of rows in the source area.
    //
    //   targetLeft:
    //     The leftmost column of the destination area.
    //
    //   targetTop:
    //     The topmost row of the destination area.
    //
    //   sourceChar:
    //     The character used to fill the source area.
    //
    //   sourceForeColor:
    //     The foreground color used to fill the source area.
    //
    //   sourceBackColor:
    //     The background color used to fill the source area.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     One or more of the parameters is less than zero. -or- sourceLeft or targetLeft
    //     is greater than or equal to System.Console.BufferWidth. -or- sourceTop or targetTop
    //     is greater than or equal to System.Console.BufferHeight. -or- sourceTop + sourceHeight
    //     is greater than or equal to System.Console.BufferHeight. -or- sourceLeft + sourceWidth
    //     is greater than or equal to System.Console.BufferWidth.
    //
    //   T:System.ArgumentException:
    //     One or both of the color parameters is not a member of the System.ConsoleColor
    //     enumeration.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.PlatformNotSupportedException:
    //     The current operating system is not Windows.
    public void MoveBufferArea(int sourceLeft, int sourceTop, int sourceWidth, int sourceHeight, int targetLeft,
        int targetTop, char sourceChar, ConsoleColor sourceForeColor, ConsoleColor sourceBackColor)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Sets the height and width of the screen buffer area to the specified values.
    //
    // Parameters:
    //   width:
    //     The width of the buffer area measured in columns.
    //
    //   height:
    //     The height of the buffer area measured in rows.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     height or width is less than or equal to zero. -or- height or width is greater
    //     than or equal to System.Int16.MaxValue. -or- width is less than System.Console.WindowLeft
    //     + System.Console.WindowWidth. -or- height is less than System.Console.WindowTop
    //     + System.Console.WindowHeight.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.PlatformNotSupportedException:
    //     The current operating system is not Windows.
    public void SetInternalBufferSize(int width, int height)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Sets the position of the console window relative to the screen buffer.
    //
    // Parameters:
    //   left:
    //     The column position of the upper left corner of the console window.
    //
    //   top:
    //     The row position of the upper left corner of the console window.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     left or top is less than zero. -or- left + System.Console.WindowWidth is greater
    //     than System.Console.BufferWidth. -or- top + System.Console.WindowHeight is greater
    //     than System.Console.BufferHeight.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.PlatformNotSupportedException:
    //     The current operating system is not Windows.
    public void SetInternalWindowPosition(int left, int top)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Sets the height and width of the console window to the specified values.
    //
    // Parameters:
    //   width:
    //     The width of the console window measured in columns.
    //
    //   height:
    //     The height of the console window measured in rows.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     width or height is less than or equal to zero. -or- width plus System.Console.WindowLeft
    //     or height plus System.Console.WindowTop is greater than or equal to System.Int16.MaxValue.
    //     -or- width or height is greater than the largest possible window width or height
    //     for the current screen resolution and console font.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.PlatformNotSupportedException:
    //     The current operating system is not Windows.
    public void SetInternalWindowSize(int width, int height)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Clears the console buffer and corresponding console window of display information.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Clear()
    {
        this._internalCharacterBuffer.Clear();
    }
}