﻿using System.Drawing;
using System.Text;
using HACC.VirtualConsoleBuffer.EventArgs;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace HACC.VirtualConsoleBuffer;

//
// Summary:
//     Represents the standard input, output, and error streams for console applications.
//     This class cannot be inherited.
public class VirtualConsoleBuffer : IAnsiConsole
{
    public delegate void ConsoleResizedHandler(object sender, ConsoleResizedEventArgs e);

    public delegate void NewFrameHandler(object sender, NewFrameEventArgs e);

    private readonly ILogger Logger;
    private CharacterBuffer InternalCharacterBuffer;

    private TerminalSettings TerminalSettings;

    public VirtualConsoleBuffer(ILogger logger, TerminalSettings? terminalSettings = null)
    {
        Logger = logger;
        TerminalSettings = terminalSettings.HasValue ? terminalSettings.Value : new TerminalSettings();
        InternalCharacterBuffer = new CharacterBuffer(
            logger,
            TerminalSettings.Columns,
            TerminalSettings.Rows);
    }

    //
    // Summary:
    //     Gets or sets the background color of the console.
    //
    // Returns:
    //     A value that specifies the background color of the console; that is, the color
    //     that appears behind each character. The default is black.
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     The color specified in a set operation is not a valid member of System.ConsoleColor.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public ConsoleColor BackgroundColor
    {
        get => TerminalSettings.TerminalBackground;
        set => TerminalSettings.TerminalBackground = value;
    }

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
        get => TerminalSettings.Rows;
        set
        {
            var eventArgs = new ConsoleResizedEventArgs(
                this,
                TerminalSettings.Columns,
                TerminalSettings.Rows,
                TerminalSettings.Columns,
                value);

            TerminalSettings.Rows = value;

            ConsoleResized?.Invoke(
                this,
                eventArgs);
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
        get => TerminalSettings.Columns;
        set
        {
            var eventArgs = new ConsoleResizedEventArgs(
                this,
                TerminalSettings.Columns,
                TerminalSettings.Rows,
                value,
                TerminalSettings.Rows);

            TerminalSettings.Rows = value;

            ConsoleResized?.Invoke(
                this,
                eventArgs);
        }
    }

    //
    // Summary:
    //     Gets a value indicating whether the CAPS LOCK keyboard toggle is turned on or
    //     turned off.
    //
    // Returns:
    //     true if CAPS LOCK is turned on; false if CAPS LOCK is turned off.
    //
    // Exceptions:
    //   T:System.PlatformNotSupportedException:
    //     The get operation is invoked on an operating system other than Windows.
    public bool CapsLock => throw new NotImplementedException();

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
                value,
                TerminalSettings.CursorPosition.Y);

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
                TerminalSettings.CursorPosition.X,
                value);

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

    //
    // Summary:
    //     Gets the standard error output stream.
    //
    // Returns:
    //     A System.IO.TextWriter that represents the standard error output stream.
    public TextWriter Error => throw new NotImplementedException();

    //
    // Summary:
    //     Gets or sets the foreground color of the console.
    //
    // Returns:
    //     A System.ConsoleColor that specifies the foreground color of the console; that
    //     is, the color of each character that is displayed. The default is gray.
    //
    // Exceptions:
    //   T:System.ArgumentException:
    //     The color specified in a set operation is not a valid member of System.ConsoleColor.
    //
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public ConsoleColor ForegroundColor
    {
        get => TerminalSettings.TerminalForeground;
        set
        {
            TerminalSettings.TerminalForeground = value;

            throw new NotImplementedException();
        }
    }

    //
    // Summary:
    //     Gets the standard input stream.
    //
    // Returns:
    //     A System.IO.TextReader that represents the standard input stream.
    public TextReader In => throw new NotImplementedException();

    //
    // Summary:
    //     Gets or sets the encoding the console uses to read input.
    //
    // Returns:
    //     The encoding used to read console input.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The property value in a set operation is null.
    //
    //   T:System.IO.IOException:
    //     An error occurred during the execution of this operation.
    //
    //   T:System.Security.SecurityException:
    //     Your application does not have permission to perform this operation.
    public Encoding InputEncoding
    {
        get => throw new NotImplementedException();
        set { }
    }

    //
    // Summary:
    //     Gets a value that indicates whether the error output stream has been redirected
    //     from the standard error stream.
    //
    // Returns:
    //     true if error output is redirected; otherwise, false.
    public bool IsErrorRedirected => throw new NotImplementedException();

    //
    // Summary:
    //     Gets a value that indicates whether input has been redirected from the standard
    //     input stream.
    //
    // Returns:
    //     true if input is redirected; otherwise, false.
    public bool IsInputRedirected => throw new NotImplementedException();

    //
    // Summary:
    //     Gets a value that indicates whether output has been redirected from the standard
    //     output stream.
    //
    // Returns:
    //     true if output is redirected; otherwise, false.
    public bool IsOutputRedirected => throw new NotImplementedException();

    //
    // Summary:
    //     Gets a value indicating whether a key press is available in the input stream.
    //
    // Returns:
    //     true if a key press is available; otherwise, false.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.InvalidOperationException:
    //     Standard input is redirected to a file instead of the keyboard.
    public bool KeyAvailable => throw new NotImplementedException();

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
    //     Gets a value indicating whether the NUM LOCK keyboard toggle is turned on or
    //     turned off.
    //
    // Returns:
    //     true if NUM LOCK is turned on; false if NUM LOCK is turned off.
    //
    // Exceptions:
    //   T:System.PlatformNotSupportedException:
    //     The get operation is invoked on an operating system other than Windows.
    public bool NumberLock => throw new NotImplementedException();

    //
    // Summary:
    //     Gets the standard output stream.
    //
    // Returns:
    //     A System.IO.TextWriter that represents the standard output stream.
    public TextWriter Out => throw new NotImplementedException();

    //
    // Summary:
    //     Gets or sets the encoding the console uses to write output.
    //
    // Returns:
    //     The encoding used to write console output.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     The property value in a set operation is null.
    //
    //   T:System.IO.IOException:
    //     An error occurred during the execution of this operation.
    //
    //   T:System.Security.SecurityException:
    //     Your application does not have permission to perform this operation.
    public Encoding OutputEncoding
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

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
        get => TerminalSettings.Title;
        set
        {
            TerminalSettings.Title = value;

            throw new NotImplementedException();
        }
    }

    //
    // Summary:
    //     Gets or sets a value indicating whether the combination of the System.ConsoleModifiers.Control
    //     modifier key and System.ConsoleKey.C console key (Ctrl+C) is treated as ordinary
    //     input or as an interruption that is handled by the operating system.
    //
    // Returns:
    //     true if Ctrl+C is treated as ordinary input; otherwise, false.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     Unable to get or set the input mode of the console input buffer.
    public bool TreatControlCAsInput
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
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
    public int WindowHeight
    {
        get => TerminalSettings.Rows;
        set
        {
            TerminalSettings.Rows = value;

            throw new NotImplementedException();
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
    public int WindowWidth
    {
        get => TerminalSettings.Columns;
        set
        {
            TerminalSettings.Columns = value;

            throw new NotImplementedException();
        }
    }

    public Profile Profile => throw new NotImplementedException();

    public IAnsiConsoleCursor Cursor => throw new NotImplementedException();

    public IAnsiConsoleInput Input => throw new NotImplementedException();

    public IExclusivityMode ExclusivityMode => new DefaultExclusivityMode();

    public RenderPipeline Pipeline => throw new NotImplementedException();

    public void Clear(bool home)
    {
        throw new NotImplementedException();
    }

    public void Write(IRenderable renderable)
    {
        throw new NotImplementedException();
    }

    public event NewFrameHandler NewFrame;
    public event ConsoleResizedHandler ConsoleResized;

    //
    // Summary:
    //     Occurs when the System.ConsoleModifiers.Control modifier key (Ctrl) and either
    //     the System.ConsoleKey.C console key (C) or the Break key are pressed simultaneously
    //     (Ctrl+C or Ctrl+Break).
    public event ConsoleCancelEventHandler? CancelKeyPress
    {
        //[System.Runtime.CompilerServices.NullableContext(2)]
        add => throw new NotImplementedException();
        //[System.Runtime.CompilerServices.NullableContext(2)]
        remove => throw new NotImplementedException();
    }

    //
    // Summary:
    //     Plays the sound of a beep through the console speaker.
    //
    // Exceptions:
    //   T:System.Security.HostProtectionException:
    //     This method was executed on a server, such as SQL Server, that does not permit
    //     access to a user interface.
    public void Beep()
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Plays the sound of a beep of a specified frequency and duration through the console
    //     speaker.
    //
    // Parameters:
    //   frequency:
    //     The frequency of the beep, ranging from 37 to 32767 hertz.
    //
    //   duration:
    //     The duration of the beep measured in milliseconds.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     frequency is less than 37 or more than 32767 hertz. -or- duration is less than
    //     or equal to zero.
    //
    //   T:System.Security.HostProtectionException:
    //     This method was executed on a server, such as SQL Server, that does not permit
    //     access to the console.
    //
    //   T:System.PlatformNotSupportedException:
    //     The current operating system is not Windows.
    public void Beep(int frequency, int duration)
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
        throw new NotImplementedException();
    }

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
    //     Acquires the standard error stream.
    //
    // Returns:
    //     The standard error stream.
    public Stream OpenStandardError()
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Acquires the standard error stream, which is set to a specified buffer size.
    //
    // Parameters:
    //   bufferSize:
    //     This parameter has no effect, but its value must be greater than or equal to
    //     zero.
    //
    // Returns:
    //     The standard error stream.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     bufferSize is less than or equal to zero.
    public Stream OpenStandardError(int bufferSize)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Acquires the standard input stream.
    //
    // Returns:
    //     The standard input stream.
    public Stream OpenStandardInput()
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Acquires the standard input stream, which is set to a specified buffer size.
    //
    // Parameters:
    //   bufferSize:
    //     This parameter has no effect, but its value must be greater than or equal to
    //     zero.
    //
    // Returns:
    //     The standard input stream.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     bufferSize is less than or equal to zero.
    public Stream OpenStandardInput(int bufferSize)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Acquires the standard output stream.
    //
    // Returns:
    //     The standard output stream.
    public Stream OpenStandardOutput()
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Acquires the standard output stream, which is set to a specified buffer size.
    //
    // Parameters:
    //   bufferSize:
    //     This parameter has no effect, but its value must be greater than or equal to
    //     zero.
    //
    // Returns:
    //     The standard output stream.
    //
    // Exceptions:
    //   T:System.ArgumentOutOfRangeException:
    //     bufferSize is less than or equal to zero.
    public Stream OpenStandardOutput(int bufferSize)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Reads the next character from the standard input stream.
    //
    // Returns:
    //     The next character from the input stream, or negative one (-1) if there are currently
    //     no more characters to be read.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public int Read()
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Obtains the next character or function key pressed by the user. The pressed key
    //     is displayed in the console window.
    //
    // Returns:
    //     An object that describes the System.ConsoleKey constant and Unicode character,
    //     if any, that correspond to the pressed console key. The System.ConsoleKeyInfo
    //     object also describes, in a bitwise combination of System.ConsoleModifiers values,
    //     whether one or more Shift, Alt, or Ctrl modifier keys was pressed simultaneously
    //     with the console key.
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     The System.Console.In property is redirected from some stream other than the
    //     console.
    public ConsoleKeyInfo ReadKey()
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Obtains the next character or function key pressed by the user. The pressed key
    //     is optionally displayed in the console window.
    //
    // Parameters:
    //   intercept:
    //     Determines whether to display the pressed key in the console window. true to
    //     not display the pressed key; otherwise, false.
    //
    // Returns:
    //     An object that describes the System.ConsoleKey constant and Unicode character,
    //     if any, that correspond to the pressed console key. The System.ConsoleKeyInfo
    //     object also describes, in a bitwise combination of System.ConsoleModifiers values,
    //     whether one or more Shift, Alt, or Ctrl modifier keys was pressed simultaneously
    //     with the console key.
    //
    // Exceptions:
    //   T:System.InvalidOperationException:
    //     The System.Console.In property is redirected from some stream other than the
    //     console.
    public ConsoleKeyInfo ReadKey(bool intercept)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Reads the next line of characters from the standard input stream.
    //
    // Returns:
    //     The next line of characters from the input stream, or null if no more lines are
    //     available.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.OutOfMemoryException:
    //     There is insufficient memory to allocate a buffer for the returned string.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     The number of characters in the next line of characters is greater than System.Int32.MaxValue.
    public string? ReadLine()
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Sets the foreground and background console colors to their defaults.
    //
    // Exceptions:
    //   T:System.Security.SecurityException:
    //     The user does not have permission to perform this action.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void ResetColor()
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
    public void SetBufferSize(int width, int height)
    {
        throw new NotImplementedException();
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

        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Sets the System.Console.Error property to the specified System.IO.TextWriter
    //     object.
    //
    // Parameters:
    //   newError:
    //     A stream that is the new standard error output.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     newError is null.
    //
    //   T:System.Security.SecurityException:
    //     The caller does not have the required permission.
    public void SetError(TextWriter newError)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Sets the System.Console.In property to the specified System.IO.TextReader object.
    //
    // Parameters:
    //   newIn:
    //     A stream that is the new standard input.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     newIn is null.
    //
    //   T:System.Security.SecurityException:
    //     The caller does not have the required permission.
    public void SetIn(TextReader newIn)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Sets the System.Console.Out property to target the System.IO.TextWriter object.
    //
    // Parameters:
    //   newOut:
    //     A text writer to be used as the new standard output.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     newOut is null.
    //
    //   T:System.Security.SecurityException:
    //     The caller does not have the required permission.
    public void SetOut(TextWriter newOut)
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
    public void SetWindowPosition(int left, int top)
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
    public void SetWindowSize(int width, int height)
    {
        throw new NotImplementedException();
    }

    //
    // Summary:
    //     Writes the text representation of the specified Boolean value to the standard
    //     output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Write(bool value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the specified Unicode character value to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Write(char value)
    {
        this.InternalCharacterBuffer.WriteChar(
            character: Convert.ToString(value),
            characterEffects: null);
    }

    //
    // Summary:
    //     Writes the specified array of Unicode characters to the standard output stream.
    //
    // Parameters:
    //   buffer:
    //     A Unicode character array.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Write(char[]? buffer)
    {
        string value = buffer is null ? string.Empty : new string(buffer);
        this.InternalCharacterBuffer.WriteLine(
            line: value,
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the specified subarray of Unicode characters to the standard output stream.
    //
    // Parameters:
    //   buffer:
    //     An array of Unicode characters.
    //
    //   index:
    //     The starting position in buffer.
    //
    //   count:
    //     The number of characters to write.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     buffer is null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index or count is less than zero.
    //
    //   T:System.ArgumentException:
    //     index plus count specify a position that is not within buffer.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Write(char[] buffer, int index, int count)
    {
        string value = new string(buffer).Substring(
            startIndex: index,
            length: count);
        this.InternalCharacterBuffer.WriteLine(
            line: value,
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified System.Decimal value to the standard
    //     output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Write(decimal value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified double-precision floating-point
    //     value to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Write(double value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified 32-bit signed integer value to
    //     the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Write(int value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified 64-bit signed integer value to
    //     the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Write(long value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified object to the standard output
    //     stream.
    //
    // Parameters:
    //   value:
    //     The value to write, or null.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Write(object? value)
    {
        string valueString = value is null ? string.Empty : Convert.ToString(value);
        this.InternalCharacterBuffer.WriteLine(
            line: valueString,
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified single-precision floating-point
    //     value to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Write(float value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the specified string value to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void Write(string? value)
    {
        string nonNullString = string.IsNullOrEmpty(value) ? string.Empty : value;
        this.InternalCharacterBuffer.WriteLine(
            line: nonNullString,
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified object to the standard output
    //     stream using the specified format information.
    //
    // Parameters:
    //   format:
    //     A composite format string.
    //
    //   arg0:
    //     An object to write using format.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.ArgumentNullException:
    //     format is null.
    //
    //   T:System.FormatException:
    //     The format specification in format is invalid.
    public void Write(string format, object? arg0)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: string.Format(format: format, arg0: arg0),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified objects to the standard output
    //     stream using the specified format information.
    //
    // Parameters:
    //   format:
    //     A composite format string.
    //
    //   arg0:
    //     The first object to write using format.
    //
    //   arg1:
    //     The second object to write using format.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.ArgumentNullException:
    //     format is null.
    //
    //   T:System.FormatException:
    //     The format specification in format is invalid.
    public void Write(string format, object? arg0, object? arg1)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: string.Format(
                format: format,
                arg0: arg0,
                arg1: arg1),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified objects to the standard output
    //     stream using the specified format information.
    //
    // Parameters:
    //   format:
    //     A composite format string.
    //
    //   arg0:
    //     The first object to write using format.
    //
    //   arg1:
    //     The second object to write using format.
    //
    //   arg2:
    //     The third object to write using format.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.ArgumentNullException:
    //     format is null.
    //
    //   T:System.FormatException:
    //     The format specification in format is invalid.
    public void Write(string format, object? arg0, object? arg1, object? arg2)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: string.Format(
                format: format,
                arg0: arg0,
                arg1: arg1,
                arg2: arg2),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified array of objects to the standard
    //     output stream using the specified format information.
    //
    // Parameters:
    //   format:
    //     A composite format string.
    //
    //   arg:
    //     An array of objects to write using format.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.ArgumentNullException:
    //     format or arg is null.
    //
    //   T:System.FormatException:
    //     The format specification in format is invalid.
    public void Write(string format, params object?[]? arg)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: string.Format(
                format: format,
                arg0: arg),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified 32-bit unsigned integer value
    //     to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    [CLSCompliant(false)]
    public void Write(uint value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the text representation of the specified 64-bit unsigned integer value
    //     to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    [CLSCompliant(false)]
    public void Write(ulong value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: false);
    }

    //
    // Summary:
    //     Writes the current line terminator to the standard output stream.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine()
    {
        this.InternalCharacterBuffer.WriteLine(
            line: "",
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the text representation of the specified Boolean value, followed by the
    //     current line terminator, to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine(bool value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the specified Unicode character, followed by the current line terminator,
    //     value to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine(char value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the specified array of Unicode characters, followed by the current line
    //     terminator, to the standard output stream.
    //
    // Parameters:
    //   buffer:
    //     A Unicode character array.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine(char[]? buffer)
    {
        string value = buffer is null ? string.Empty : Convert.ToString(buffer);
        this.InternalCharacterBuffer.WriteLine(
            line: value,
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the specified subarray of Unicode characters, followed by the current
    //     line terminator, to the standard output stream.
    //
    // Parameters:
    //   buffer:
    //     An array of Unicode characters.
    //
    //   index:
    //     The starting position in buffer.
    //
    //   count:
    //     The number of characters to write.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     buffer is null.
    //
    //   T:System.ArgumentOutOfRangeException:
    //     index or count is less than zero.
    //
    //   T:System.ArgumentException:
    //     index plus count specify a position that is not within buffer.
    //
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine(char[] buffer, int index, int count)
    {
        string value = Convert.ToString(buffer)
            .Substring(
                startIndex: index,
                length: count);
        this.InternalCharacterBuffer.WriteLine(
            line: value,
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the text representation of the specified System.Decimal value, followed
    //     by the current line terminator, to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine(decimal value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the text representation of the specified double-precision floating-point
    //     value, followed by the current line terminator, to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine(double value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the text representation of the specified 32-bit signed integer value,
    //     followed by the current line terminator, to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine(int value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the text representation of the specified 64-bit signed integer value,
    //     followed by the current line terminator, to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine(long value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the text representation of the specified object, followed by the current
    //     line terminator, to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine(object? value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the text representation of the specified single-precision floating-point
    //     value, followed by the current line terminator, to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine(float value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the specified string value, followed by the current line terminator, to
    //     the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    public void WriteLine(string? value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: value,
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the text representation of the specified object, followed by the current
    //     line terminator, to the standard output stream using the specified format information.
    //
    // Parameters:
    //   format:
    //     A composite format string.
    //
    //   arg0:
    //     An object to write using format.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.ArgumentNullException:
    //     format is null.
    //
    //   T:System.FormatException:
    //     The format specification in format is invalid.
    public void WriteLine(string format, object? arg0)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: string.Format(
                format: format,
                arg0: arg0),
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the text representation of the specified objects, followed by the current
    //     line terminator, to the standard output stream using the specified format information.
    //
    // Parameters:
    //   format:
    //     A composite format string.
    //
    //   arg0:
    //     The first object to write using format.
    //
    //   arg1:
    //     The second object to write using format.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.ArgumentNullException:
    //     format is null.
    //
    //   T:System.FormatException:
    //     The format specification in format is invalid.
    public void WriteLine(string format, object? arg0, object? arg1)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: string.Format(
                format: format,
                arg0: arg0,
                arg1: arg1),
            characterEffects: null,
            automaticWrap: true);    }

    //
    // Summary:
    //     Writes the text representation of the specified objects, followed by the current
    //     line terminator, to the standard output stream using the specified format information.
    //
    // Parameters:
    //   format:
    //     A composite format string.
    //
    //   arg0:
    //     The first object to write using format.
    //
    //   arg1:
    //     The second object to write using format.
    //
    //   arg2:
    //     The third object to write using format.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.ArgumentNullException:
    //     format is null.
    //
    //   T:System.FormatException:
    //     The format specification in format is invalid.
    public void WriteLine(string format, object? arg0, object? arg1, object? arg2)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: string.Format(
                format: format,
                arg0: arg0,
                arg1: arg1,
                arg2: arg2),
            characterEffects: null,
            automaticWrap: true);    }

    //
    // Summary:
    //     Writes the text representation of the specified array of objects, followed by
    //     the current line terminator, to the standard output stream using the specified
    //     format information.
    //
    // Parameters:
    //   format:
    //     A composite format string.
    //
    //   arg:
    //     An array of objects to write using format.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    //
    //   T:System.ArgumentNullException:
    //     format or arg is null.
    //
    //   T:System.FormatException:
    //     The format specification in format is invalid.
    public void WriteLine(string format, params object?[]? arg)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: string.Format(
                format: format,
                arg0: arg),
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the text representation of the specified 32-bit unsigned integer value,
    //     followed by the current line terminator, to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    [CLSCompliant(false)]
    public void WriteLine(uint value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: true);
    }

    //
    // Summary:
    //     Writes the text representation of the specified 64-bit unsigned integer value,
    //     followed by the current line terminator, to the standard output stream.
    //
    // Parameters:
    //   value:
    //     The value to write.
    //
    // Exceptions:
    //   T:System.IO.IOException:
    //     An I/O error occurred.
    [CLSCompliant(false)]
    public void WriteLine(ulong value)
    {
        this.InternalCharacterBuffer.WriteLine(
            line: Convert.ToString(value),
            characterEffects: null,
            automaticWrap: true);
    }
}