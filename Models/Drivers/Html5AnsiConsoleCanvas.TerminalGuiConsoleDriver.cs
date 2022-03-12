using NStack;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;

namespace HACC.Models.Drivers;

public partial class Html5AnsiConsoleCanvas
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public override int Cols => this.WindowColumns;

    public override int Rows => this.WindowRows;

    // Only handling left here because not all terminals has a horizontal scroll bar.
    public override int Left => this.WindowLeft;
    public override int Top => this.WindowTop;
    public override bool HeightAsBuffer { get; set; }
    public override WebClipboard Clipboard { get; }

    // The format is rows, columns and 3 values on the last column: Rune, Attribute and Dirty Flag
    private bool[] dirtyLine;

    /// <summary>
    ///     Assists with testing, the format is rows, columns and 3 values on the last column: Rune, Attribute and Dirty Flag
    /// </summary>
    public int[,,] Contents { get; private set; }

    private void UpdateOffscreen()
    {
        var cols = this.Cols;
        var rows = this.Rows;

        this.Contents = new int [rows, cols, 3];
        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < cols; c++)
            {
                this.Contents[r,
                    c,
                    0] = ' ';
                this.Contents[r,
                    c,
                    1] = MakeColor(f: ConsoleColor.Gray,
                    b: ConsoleColor.Black);
                this.Contents[r,
                    c,
                    2] = 0;
            }
        }

        this.dirtyLine = new bool [rows];
        for (var row = 0; row < rows; row++)
        {
            this.dirtyLine[row] = true;
        }
    }

    private static readonly bool sync = false;


    private bool needMove;

    // Current row, and current col, tracked by Move/AddCh only
    private int ccol, crow;

    public override void Move(int col, int row)
    {
        this.ccol = col;
        this.crow = row;

        if (this.Clip.Contains(x: col,
                y: row))
        {
            this.CursorTop = row;
            this.CursorLeft = col;
            this.needMove = false;
        }
        else
        {
            this.CursorTop = this.Clip.Y;
            this.CursorLeft = this.Clip.X;
            this.needMove = true;
        }
    }

    public override void AddRune(Rune rune)
    {
        rune = MakePrintable(c: rune);
        if (this.Clip.Contains(x: this.ccol,
                y: this.crow))
        {
            if (this.needMove)
                //MockConsole.CursorLeft = ccol;
                //MockConsole.CursorTop = crow;
                this.needMove = false;

            this.Contents[this.crow, this.ccol,
                0] = (int) (uint) rune;
            this.Contents[this.crow, this.ccol,
                1] = this.currentAttribute;
            this.Contents[this.crow, this.ccol,
                2] = 1;
            this.dirtyLine[this.crow] = true;
        }
        else
        {
            this.needMove = true;
        }

        this.ccol++;
        //if (ccol == Cols) {
        //	ccol = 0;
        //	if (crow + 1 < Rows)
        //		crow++;
        //}
        if (sync) this.UpdateScreen();
    }

    public override void AddStr(ustring str)
    {
        foreach (var rune in str)
        {
            this.AddRune(rune: rune);
        }
    }

    public override void End()
    {
        this.ResetColor();
        this.Clear();
    }

    private static Attribute MakeColor(ConsoleColor f, ConsoleColor b)
    {
        // Encode the colors into the int value.
        return new Attribute(
            value: (((int) f & 0xffff) << 16) | ((int) b & 0xffff),
            foreground: (Color) f,
            background: (Color) b
        );
    }

    public override void Init(Action terminalResized)
    {
        this._terminalSettings = new TerminalSettings();
        this.TerminalResized = terminalResized;
        this.Clear();
        this.ResizeScreen();
        this.UpdateOffScreen();

        Colors.TopLevel = new ColorScheme();
        Colors.Base = new ColorScheme();
        Colors.Dialog = new ColorScheme();
        Colors.Menu = new ColorScheme();
        Colors.Error = new ColorScheme();
        this.Clip = new Rect(x: 0,
            y: 0,
            width: this.Cols,
            height: this.Rows);

        Colors.TopLevel.Normal = MakeColor(f: ConsoleColor.Green,
            b: ConsoleColor.Black);
        Colors.TopLevel.Focus = MakeColor(f: ConsoleColor.White,
            b: ConsoleColor.DarkCyan);
        Colors.TopLevel.HotNormal = MakeColor(f: ConsoleColor.DarkYellow,
            b: ConsoleColor.Black);
        Colors.TopLevel.HotFocus = MakeColor(f: ConsoleColor.DarkBlue,
            b: ConsoleColor.DarkCyan);
        Colors.TopLevel.Disabled = MakeColor(f: ConsoleColor.DarkGray,
            b: ConsoleColor.Black);

        Colors.Base.Normal = MakeColor(f: ConsoleColor.White,
            b: ConsoleColor.Blue);
        Colors.Base.Focus = MakeColor(f: ConsoleColor.Black,
            b: ConsoleColor.Cyan);
        Colors.Base.HotNormal = MakeColor(f: ConsoleColor.Yellow,
            b: ConsoleColor.Blue);
        Colors.Base.HotFocus = MakeColor(f: ConsoleColor.Yellow,
            b: ConsoleColor.Cyan);
        Colors.Base.Disabled = MakeColor(f: ConsoleColor.DarkGray,
            b: ConsoleColor.DarkBlue);

        // Focused,
        //    Selected, Hot: Yellow on Black
        //    Selected, text: white on black
        //    Unselected, hot: yellow on cyan
        //    unselected, text: same as unfocused
        Colors.Menu.HotFocus = MakeColor(f: ConsoleColor.Yellow,
            b: ConsoleColor.Black);
        Colors.Menu.Focus = MakeColor(f: ConsoleColor.White,
            b: ConsoleColor.Black);
        Colors.Menu.HotNormal = MakeColor(f: ConsoleColor.Yellow,
            b: ConsoleColor.Cyan);
        Colors.Menu.Normal = MakeColor(f: ConsoleColor.White,
            b: ConsoleColor.Cyan);
        Colors.Menu.Disabled = MakeColor(f: ConsoleColor.DarkGray,
            b: ConsoleColor.Cyan);

        Colors.Dialog.Normal = MakeColor(f: ConsoleColor.Black,
            b: ConsoleColor.Gray);
        Colors.Dialog.Focus = MakeColor(f: ConsoleColor.Black,
            b: ConsoleColor.Cyan);
        Colors.Dialog.HotNormal = MakeColor(f: ConsoleColor.Blue,
            b: ConsoleColor.Gray);
        Colors.Dialog.HotFocus = MakeColor(f: ConsoleColor.Blue,
            b: ConsoleColor.Cyan);
        Colors.Dialog.Disabled = MakeColor(f: ConsoleColor.DarkGray,
            b: ConsoleColor.Gray);

        Colors.Error.Normal = MakeColor(f: ConsoleColor.White,
            b: ConsoleColor.Red);
        Colors.Error.Focus = MakeColor(f: ConsoleColor.Black,
            b: ConsoleColor.Gray);
        Colors.Error.HotNormal = MakeColor(f: ConsoleColor.Yellow,
            b: ConsoleColor.Red);
        Colors.Error.HotFocus = Colors.Error.HotNormal;
        Colors.Error.Disabled = MakeColor(f: ConsoleColor.DarkGray,
            b: ConsoleColor.White);

        //MockConsole.Clear ();
    }

    public override Attribute MakeAttribute(Color fore, Color back)
    {
        return MakeColor(f: (ConsoleColor) fore,
            b: (ConsoleColor) back);
    }

    private int redrawColor = -1;

    private void SetColor(int color)
    {
        this.redrawColor = color;
        var values = Enum.GetValues(enumType: typeof(ConsoleColor))
            .OfType<ConsoleColor>()
            .Select(selector: s => (int) s);
        if (values.Contains(value: color & 0xffff)) this.BackgroundColor = (ConsoleColor) (color & 0xffff);

        if (values.Contains(value: (color >> 16) & 0xffff))
            this.ForegroundColor = (ConsoleColor) ((color >> 16) & 0xffff);
    }

    public override void UpdateScreen()
    {
        var top = this.Top;
        var left = this.Left;
        var rows = Math.Min(val1: this.WindowRows + top,
            val2: this.Rows);
        var cols = this.Cols;

        this.CursorTop = 0;
        this.CursorLeft = 0;
        for (var row = top; row < rows; row++)
        {
            this.dirtyLine[row] = false;
            for (var col = left; col < cols; col++)
            {
                this.Contents[row,
                    col,
                    2] = 0;
                var color = this.Contents[row,
                    col,
                    1];
                if (color != this.redrawColor) this.SetColor(color: color);
                this.Write(value: (char) this.Contents[row,
                    col,
                    0]);
            }
        }
    }

    public override void Refresh()
    {
        var rows = this.Rows;
        var cols = this.Cols;

        var savedRow = this.CursorTop;
        var savedCol = this.CursorLeft;
        for (var row = 0; row < rows; row++)
        {
            if (!this.dirtyLine[row])
                continue;
            this.dirtyLine[row] = false;
            for (var col = 0; col < cols; col++)
            {
                if (this.Contents[row,
                        col,
                        2] != 1)
                    continue;

                this.CursorTop = row;
                this.CursorLeft = col;
                for (;
                     col < cols && this.Contents[row,
                         col,
                         2] == 1;
                     col++)
                {
                    var color = this.Contents[row,
                        col,
                        1];
                    if (color != this.redrawColor) this.SetColor(color: color);

                    this.Write(value: (char) this.Contents[row,
                        col,
                        0]);
                    this.Contents[row,
                        col,
                        2] = 0;
                }
            }
        }

        this.CursorTop = savedRow;
        this.CursorLeft = savedCol;
    }

    private Attribute currentAttribute;

    public override void SetAttribute(Attribute c)
    {
        this.currentAttribute = c;
    }

    private Key MapKey(ConsoleKeyInfo keyInfo)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.Escape:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.Esc);
            case ConsoleKey.Tab:
                return keyInfo.Modifiers == ConsoleModifiers.Shift ? Key.BackTab : Key.Tab;
            case ConsoleKey.Home:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.Home);
            case ConsoleKey.End:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.End);
            case ConsoleKey.LeftArrow:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.CursorLeft);
            case ConsoleKey.RightArrow:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.CursorRight);
            case ConsoleKey.UpArrow:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.CursorUp);
            case ConsoleKey.DownArrow:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.CursorDown);
            case ConsoleKey.PageUp:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.PageUp);
            case ConsoleKey.PageDown:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.PageDown);
            case ConsoleKey.Enter:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.Enter);
            case ConsoleKey.Spacebar:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: keyInfo.KeyChar == 0 ? Key.Space : (Key) keyInfo.KeyChar);
            case ConsoleKey.Backspace:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.Backspace);
            case ConsoleKey.Delete:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.DeleteChar);
            case ConsoleKey.Insert:
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.InsertChar);

            case ConsoleKey.Oem1:
            case ConsoleKey.Oem2:
            case ConsoleKey.Oem3:
            case ConsoleKey.Oem4:
            case ConsoleKey.Oem5:
            case ConsoleKey.Oem6:
            case ConsoleKey.Oem7:
            case ConsoleKey.Oem8:
            case ConsoleKey.Oem102:
            case ConsoleKey.OemPeriod:
            case ConsoleKey.OemComma:
            case ConsoleKey.OemPlus:
            case ConsoleKey.OemMinus:
                if (keyInfo.KeyChar == 0)
                    return Key.Unknown;

                return (Key) keyInfo.KeyChar;
        }

        var key = keyInfo.Key;
        if (key >= ConsoleKey.A && key <= ConsoleKey.Z)
        {
            var delta = key - ConsoleKey.A;
            if (keyInfo.Modifiers == ConsoleModifiers.Control)
                return (Key) ((uint) Key.CtrlMask | ((uint) Key.A + delta));

            if (keyInfo.Modifiers == ConsoleModifiers.Alt) return (Key) ((uint) Key.AltMask | ((uint) Key.A + delta));

            if ((keyInfo.Modifiers & (ConsoleModifiers.Alt | ConsoleModifiers.Control)) != 0)
            {
                if (keyInfo.KeyChar == 0)
                    return (Key) ((uint) Key.AltMask | (uint) Key.CtrlMask | ((uint) Key.A + delta));
                return (Key) keyInfo.KeyChar;
            }

            return (Key) keyInfo.KeyChar;
        }

        if (key >= ConsoleKey.D0 && key <= ConsoleKey.D9)
        {
            var delta = key - ConsoleKey.D0;
            if (keyInfo.Modifiers == ConsoleModifiers.Alt) return (Key) ((uint) Key.AltMask | ((uint) Key.D0 + delta));

            if (keyInfo.Modifiers == ConsoleModifiers.Control)
                return (Key) ((uint) Key.CtrlMask | ((uint) Key.D0 + delta));

            if (keyInfo.KeyChar == 0 || keyInfo.KeyChar == 30)
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: (Key) ((uint) Key.D0 + delta));

            return (Key) keyInfo.KeyChar;
        }

        if (key >= ConsoleKey.F1 && key <= ConsoleKey.F12)
        {
            var delta = key - ConsoleKey.F1;
            if ((keyInfo.Modifiers & (ConsoleModifiers.Shift | ConsoleModifiers.Alt | ConsoleModifiers.Control)) != 0)
                return this.MapKeyModifiers(keyInfo: keyInfo,
                    key: (Key) ((uint) Key.F1 + delta));

            return (Key) ((uint) Key.F1 + delta);
        }

        if (keyInfo.KeyChar != 0)
            return this.MapKeyModifiers(keyInfo: keyInfo,
                key: (Key) keyInfo.KeyChar);

        return (Key) 0xffffffff;
    }

    private KeyModifiers keyModifiers;

    private Key MapKeyModifiers(ConsoleKeyInfo keyInfo, Key key)
    {
        var keyMod = new Key();
        if ((keyInfo.Modifiers & ConsoleModifiers.Shift) != 0)
            keyMod = Key.ShiftMask;
        if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
            keyMod |= Key.CtrlMask;
        if ((keyInfo.Modifiers & ConsoleModifiers.Alt) != 0)
            keyMod |= Key.AltMask;

        return keyMod != Key.Null ? keyMod | key : key;
    }

    private Action<KeyEvent> keyHandler;
    private Action<KeyEvent> keyUpHandler;

    public override void PrepareToRun(MainLoop mainLoop, Action<KeyEvent> keyHandler, Action<KeyEvent> keyDownHandler,
        Action<KeyEvent> keyUpHandler, Action<MouseEvent> mouseHandler)
    {
        this.keyHandler = keyHandler;
        this.keyUpHandler = keyUpHandler;

        // Note: Net doesn't support keydown/up events and thus any passed keyDown/UpHandlers will never be called
        (mainLoop.Driver as FakeMainLoop).KeyPressed += consoleKey => this.ProcessInput(consoleKey: consoleKey);
    }

    private void ProcessInput(ConsoleKeyInfo consoleKey)
    {
        this.keyModifiers = new KeyModifiers();
        var map = this.MapKey(keyInfo: consoleKey);
        if (map == (Key) 0xffffffff)
            return;

        if (consoleKey.Modifiers.HasFlag(flag: ConsoleModifiers.Alt)) this.keyModifiers.Alt = true;

        if (consoleKey.Modifiers.HasFlag(flag: ConsoleModifiers.Shift)) this.keyModifiers.Shift = true;

        if (consoleKey.Modifiers.HasFlag(flag: ConsoleModifiers.Control)) this.keyModifiers.Ctrl = true;

        this.keyHandler(obj: new KeyEvent(k: map,
            km: this.keyModifiers));
        this.keyUpHandler(obj: new KeyEvent(k: map,
            km: this.keyModifiers));
    }

    public override Attribute GetAttribute()
    {
        return this.currentAttribute;
    }

    /// <inheritdoc />
    public override bool GetCursorVisibility(out CursorVisibility visibility)
    {
        if (this.CursorVisible)
            visibility = CursorVisibility.Default;
        else
            visibility = CursorVisibility.Invisible;

        return false;
    }

    /// <inheritdoc />
    public override bool SetCursorVisibility(CursorVisibility visibility)
    {
        if (visibility == CursorVisibility.Invisible)
            this.CursorVisible = false;
        else
            this.CursorVisible = true;

        return false;
    }

    /// <inheritdoc />
    public override bool EnsureCursorVisibility()
    {
        return false;
    }

    public override void SendKeys(char keyChar, ConsoleKey key, bool shift, bool alt, bool control)
    {
        this.ProcessInput(consoleKey: new ConsoleKeyInfo(keyChar: keyChar,
            key: key,
            shift: shift,
            alt: alt,
            control: control));
    }

    public void SetBufferSize(int width, int height)
    {
        this._terminalSettings.BufferRows = height;
        this._terminalSettings.BufferColumns = width;
        this.WindowRows = height;
        this.WindowColumns = width;
        this.ProcessResize();
    }

    public void SetWindowSize(int width, int height)
    {
        this.WindowColumns = width;
        this.WindowRows = height;
        if (width > this.BufferWidth || !this.HeightAsBuffer) this.BufferWidth = width;

        if (height > this.BufferHeight || !this.HeightAsBuffer) this.BufferHeight = height;

        this.ProcessResize();
    }

    public void SetWindowPosition(int left, int top)
    {
        if (this.HeightAsBuffer)
        {
            this.WindowLeft = this.WindowLeft = Math.Max(val1: Math.Min(val1: left,
                    val2: this.Cols - this.WindowColumns),
                val2: 0);
            this.WindowTop = this.WindowTop = Math.Max(val1: Math.Min(val1: top,
                    val2: this.Rows - this.WindowRows),
                val2: 0);
        }
        else if (this.WindowLeft > 0 || this.WindowTop > 0)
        {
            this.WindowLeft = 0;
            this.WindowTop = 0;
        }
    }

    private void ProcessResize()
    {
        this.ResizeScreen();
        this.UpdateOffScreen();
        this.TerminalResized?.Invoke();
    }

    private void ResizeScreen()
    {
        if (!this.HeightAsBuffer)
        {
            if (this.WindowRows > 0)
                // Can raise an exception while is still resizing.
                try
                {
#pragma warning disable CA1416
                    this.CursorTop = 0;
                    this.CursorLeft = 0;
                    this.WindowTop = 0;
                    this.WindowLeft = 0;
#pragma warning restore CA1416
                }
                catch (IOException)
                {
                    return;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return;
                }
        }
        else
        {
            try
            {
#pragma warning disable CA1416
                this.WindowLeft = Math.Max(val1: Math.Min(val1: this.Left,
                        val2: this.Cols - this.WindowColumns),
                    val2: 0);
                this.WindowTop = Math.Max(val1: Math.Min(val1: this.Top,
                        val2: this.Rows - this.WindowRows),
                    val2: 0);
#pragma warning restore CA1416
            }
            catch (Exception)
            {
                return;
            }
        }

        this.Clip = new Rect(x: 0,
            y: 0,
            width: this.Cols,
            height: this.Rows);

        this.Contents = new int [this.Rows, this.Cols, 3];
        this.dirtyLine = new bool [this.Rows];
    }

    private void UpdateOffScreen()
    {
        // Can raise an exception while is still resizing.
        try
        {
            for (var row = 0; row < this.Rows; row++)
            {
                for (var c = 0; c < this.Cols; c++)
                {
                    this.Contents[row,
                        c,
                        0] = ' ';
                    this.Contents[row,
                        c,
                        1] = Colors.TopLevel.Normal;
                    this.Contents[row,
                        c,
                        2] = 0;
                    this.dirtyLine[row] = true;
                }
            }
        }
        catch (IndexOutOfRangeException)
        {
        }
    }

    public override bool GetColors(int value, out Color foreground, out Color background)
    {
        var hasColor = false;
        foreground = default;
        background = default;
        var values = Enum.GetValues(enumType: typeof(ConsoleColor))
            .OfType<ConsoleColor>()
            .Select(selector: s => (int) s);
        if (values.Contains(value: value & 0xffff))
        {
            hasColor = true;
            background = (Color) (ConsoleColor) (value & 0xffff);
        }

        if (values.Contains(value: (value >> 16) & 0xffff))
        {
            hasColor = true;
            foreground = (Color) (ConsoleColor) ((value >> 16) & 0xffff);
        }

        return hasColor;
    }

    #region Unused

    public override void UpdateCursor()
    {
        //
    }

    public override void StartReportingMouseMoves()
    {
    }

    public override void StopReportingMouseMoves()
    {
    }

    public override void Suspend()
    {
    }

    public override void SetColors(ConsoleColor foreground, ConsoleColor background)
    {
    }

    public override void SetColors(short foregroundColorId, short backgroundColorId)
    {
        throw new NotImplementedException();
    }

    public override void CookMouse()
    {
    }

    public override void UncookMouse()
    {
    }

    #endregion

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}