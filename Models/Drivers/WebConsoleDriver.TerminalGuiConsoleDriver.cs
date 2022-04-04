using HACC.Models.Enums;
using HACC.Models.EventArgs;
using HACC.Models.Structs;
using NStack;
using Terminal.Gui;
using Attribute = Terminal.Gui.Attribute;
using MouseEvent = Terminal.Gui.MouseEvent;

namespace HACC.Models.Drivers;

public partial class WebConsoleDriver
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    ///     Shortcut to <see cref="WebConsoleDriver.WindowColumns" />
    /// </summary>
    public override int Cols => this.WindowColumns;

    /// <summary>
    ///     Shortcut to <see cref="WebConsoleDriver.WindowRows" />
    /// </summary>
    public override int Rows => this.WindowRows;

    /// <summary>
    ///     Shortcut to <see cref="WebConsoleDriver.WindowLeft" />
    ///     Only handling left here because not all terminals has a horizontal scroll bar.
    /// </summary>
    public override int Left => this.WindowLeft;

    /// <summary>
    ///     Shortcut to <see cref="WebConsoleDriver.WindowTop" />
    /// </summary>
    public override int Top => this.WindowTop;

    /// <summary>
    ///     If false height is measured by the window height and thus no scrolling.
    ///     If true then height is measured by the buffer height, enabling scrolling.
    ///     The current <see cref="ConsoleDriver.HeightAsBuffer" /> used in the terminal.
    /// </summary>
    public override bool HeightAsBuffer { get; set; }

    public override WebClipboard Clipboard { get; }

    // The format is rows, columns and 3 values on the last column: Rune, Attribute and Dirty Flag
    private bool[] _dirtyLine;

    /// <summary>
    ///     Assists with testing, the format is rows, columns and 3 values on the last column: Rune, Attribute and Dirty Flag
    /// </summary>
    private int[,,] Contents { get; set; }

    internal bool firstRender = true;

    // ReSharper disable once UnusedMember.Local
    private void UpdateOffscreen()
    {
        var cols = this.Cols;
        var rows = this.Rows;

        // ReSharper disable once HeapView.ObjectAllocation.Evident
        this.Contents = new int[rows, cols, 3];
        for (var r = 0; r < rows; r++)
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

        this._dirtyLine = new bool[rows];
        for (var row = 0; row < rows; row++) this._dirtyLine[row] = true;
    }

    private static readonly bool sync = false;


    private bool _needMove;

    public override void Move(int col, int row)
    {
        this.TerminalSettings.SetCursorPosition(x: col,
            y: row);

        if (this.Clip.Contains(x: col,
                y: row))
        {
            this.CursorTop = row;
            this.CursorLeft = col;
            this._needMove = false;
        }
        else
        {
            this.CursorTop = this.Clip.Y;
            this.CursorLeft = this.Clip.X;
            this._needMove = true;
        }
    }

    public override void AddRune(Rune rune)
    {
        var currentPosition = this.TerminalSettings.CursorPosition;
        rune = MakePrintable(c: rune);
        if (this.Clip.Contains(x: currentPosition.X,
                y: currentPosition.Y))
        {
            if (this._needMove)
                //MockConsole.CursorLeft = ccol;
                //MockConsole.CursorTop = crow;
                this._needMove = false;

            this.Contents[currentPosition.Y,
                currentPosition.X,
                0] = (int)(uint)rune;
            this.Contents[currentPosition.Y,
                currentPosition.X,
                1] = this._currentAttribute;
            this.Contents[currentPosition.Y,
                currentPosition.X,
                2] = 1;
            this._dirtyLine[currentPosition.Y] = true;
        }
        else
        {
            this._needMove = true;
        }

        this.TerminalSettings.SetCursorPosition(x: currentPosition.X + 1,
            y: currentPosition.Y);
        //if (ccol == Cols) {
        //	ccol = 0;
        //	if (crow + 1 < WindowRows)
        //		crow++;
        //}
        if (sync) this.UpdateScreen();
    }

    public override void AddStr(ustring str)
    {
        foreach (var rune in str) this.AddRune(rune: rune);
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
            value: (((int)f & 0xffff) << 16) | ((int)b & 0xffff),
            foreground: (Color)f,
            background: (Color)b
        );
    }

    public override void Init(Action terminalResized)
    {
        this.TerminalSettings = new TerminalSettings();
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
        return MakeColor(f: (ConsoleColor)fore,
            b: (ConsoleColor)back);
    }

    private int _redrawColor = -1;

    private void SetColor(int color)
    {
        this._redrawColor = color;
        var values = Enum.GetValues(enumType: typeof(ConsoleColor))
            .OfType<ConsoleColor>()
            .Select(selector: s => (int)s);
        var enumerable = values as int[] ?? values.ToArray();
        if (enumerable.Contains(value: color & 0xffff)) this.BackgroundColor = (ConsoleColor)(color & 0xffff);

        if (enumerable.Contains(value: (color >> 16) & 0xffff))
            this.ForegroundColor = (ConsoleColor)((color >> 16) & 0xffff);
    }

    public override void UpdateScreen()
    {
        if (firstRender)
        {
            return;
        }

        lock (this.Contents)
        {
            //var top = this.Top;
            //var left = this.Left;
            //var rows = Math.Min(val1: this.WindowRows + top,
            //    val2: this.Rows);
            //var cols = this.Cols;

            //this.CursorTop = 0;
            //this.CursorLeft = 0;
            //for (var row = top; row < rows; row++)
            //{
            //    this._dirtyLine[row] = false;
            //    for (var col = left; col < cols; col++)
            //    {
            //        this.Contents[row,
            //            col,
            //            2] = 0;
            //        var color = this.Contents[row,
            //            col,
            //            1];
            //        if (color != this._redrawColor) this.SetColor(color: color);
            //        this.Write(value: (char) this.Contents[row,
            //            col,
            //            0]);
            //    }
            //}

            Task.Run(() => this._webConsole.DrawUpdatesToCanvas(
                buffer: this.Contents,
                firstRender: firstRender));

            //var task = this._webConsole.DrawUpdatesToCanvas(
            //    buffer: this.Contents,
            //    firstRender: firstRender);
            // ReSharper disable once HeapView.ObjectAllocation.Evident
            //this.NewFrame(sender: this,
            //    e: new NewFrameEventArgs(sender: this));
            //task.RunSynchronously();
        }
    }

    public override void Refresh()
    {
        UpdateScreen();
        return;

        var rows = this.Rows;
        var cols = this.Cols;

        var savedRow = this.CursorTop;
        var savedCol = this.CursorLeft;
        for (var row = 0; row < rows; row++)
        {
            if (!this._dirtyLine[row])
                continue;
            this._dirtyLine[row] = false;
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
                    if (color != this._redrawColor) this.SetColor(color: color);

                    this.Write(value: (char)this.Contents[row,
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

    private Attribute _currentAttribute;

    public override void SetAttribute(Attribute c)
    {
        this._currentAttribute = c;
    }

    private static Key MapKey(ConsoleKeyInfo keyInfo)
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (keyInfo.Key)
        {
            case ConsoleKey.Escape:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.Esc);
            case ConsoleKey.Tab:
                return keyInfo.Modifiers == ConsoleModifiers.Shift ? Key.BackTab : Key.Tab;
            case ConsoleKey.Home:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.Home);
            case ConsoleKey.End:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.End);
            case ConsoleKey.LeftArrow:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.CursorLeft);
            case ConsoleKey.RightArrow:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.CursorRight);
            case ConsoleKey.UpArrow:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.CursorUp);
            case ConsoleKey.DownArrow:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.CursorDown);
            case ConsoleKey.PageUp:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.PageUp);
            case ConsoleKey.PageDown:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.PageDown);
            case ConsoleKey.Enter:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.Enter);
            case ConsoleKey.Spacebar:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: keyInfo.KeyChar == 0 ? Key.Space : (Key)keyInfo.KeyChar);
            case ConsoleKey.Backspace:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.Backspace);
            case ConsoleKey.Delete:
                return MapKeyModifiers(keyInfo: keyInfo,
                    key: Key.DeleteChar);
            case ConsoleKey.Insert:
                return MapKeyModifiers(keyInfo: keyInfo,
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

                return (Key)keyInfo.KeyChar;
        }

        var key = keyInfo.Key;
        switch (key)
        {
            case >= ConsoleKey.A and <= ConsoleKey.Z:
                {
                    var delta = key - ConsoleKey.A;
                    switch (keyInfo.Modifiers)
                    {
                        case ConsoleModifiers.Control:
                            return (Key)((uint)Key.CtrlMask | ((uint)Key.A + delta));
                        case ConsoleModifiers.Alt:
                            return (Key)((uint)Key.AltMask | ((uint)Key.A + delta));
                    }

                    if ((keyInfo.Modifiers & (ConsoleModifiers.Alt | ConsoleModifiers.Control)) == 0)
                        return (Key)keyInfo.KeyChar;
                    if (keyInfo.KeyChar == 0)
                        return (Key)((uint)Key.AltMask | (uint)Key.CtrlMask | ((uint)Key.A + delta));
                    return (Key)keyInfo.KeyChar;
                }
            case >= ConsoleKey.D0 and <= ConsoleKey.D9:
                {
                    var delta = key - ConsoleKey.D0;
                    // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
                    switch (keyInfo.Modifiers)
                    {
                        case ConsoleModifiers.Alt:
                            return (Key)((uint)Key.AltMask | ((uint)Key.D0 + delta));
                        case ConsoleModifiers.Control:
                            return (Key)((uint)Key.CtrlMask | ((uint)Key.D0 + delta));
                    }

                    if (keyInfo.KeyChar == 0 || keyInfo.KeyChar == 30)
                        return MapKeyModifiers(keyInfo: keyInfo,
                            key: (Key)((uint)Key.D0 + delta));

                    return (Key)keyInfo.KeyChar;
                }
            case >= ConsoleKey.F1 and <= ConsoleKey.F12:
                {
                    var delta = key - ConsoleKey.F1;
                    if ((keyInfo.Modifiers & (ConsoleModifiers.Shift | ConsoleModifiers.Alt | ConsoleModifiers.Control)) !=
                        0)
                        return MapKeyModifiers(keyInfo: keyInfo,
                            key: (Key)((uint)Key.F1 + delta));

                    return (Key)((uint)Key.F1 + delta);
                }
        }

        if (keyInfo.KeyChar != 0)
            return MapKeyModifiers(keyInfo: keyInfo,
                key: (Key)keyInfo.KeyChar);

        return (Key)0xffffffff;
    }

    private KeyModifiers? _keyModifiers;

    private static Key MapKeyModifiers(ConsoleKeyInfo keyInfo, Key key)
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

    private Action<KeyEvent>? _keyHandler;
    private Action<KeyEvent>? _keyDownHandler;
    private Action<KeyEvent>? _keyUpHandler;
    private Action<MouseEvent>? _mouseHandler;

    public override void PrepareToRun(MainLoop mainLoop, Action<KeyEvent> keyHandler, Action<KeyEvent> keyDownHandler,
        Action<KeyEvent> keyUpHandler, Action<MouseEvent> mouseHandler)
    {
        this._keyHandler = keyHandler;
        this._keyDownHandler = keyDownHandler;
        this._keyUpHandler = keyUpHandler;
        this._mouseHandler = mouseHandler;

        // ReSharper disable once HeapView.DelegateAllocation
        // Note: Net doesn't support keydown/up events and thus any passed keyDown/UpHandlers will never be called
        (mainLoop.Driver as WebMainLoopDriver)!.ProcessInput += this.ProcessInput;
    }

    private void ProcessInput(InputResult inputEvent)
    {
        switch (inputEvent.EventType)
        {
            case EventType.Key:
                _keyModifiers = new KeyModifiers();
                var map = MapKey(inputEvent.KeyEvent.ConsoleKeyInfo);
                if (map == (Key)0xffffffff)
                {
                    KeyEvent key = new KeyEvent();

                    if (inputEvent.KeyEvent.KeyDown)
                    {
                        _keyDownHandler(key);
                    }
                    else
                    {
                        _keyUpHandler(key);
                    }
                }
                else
                {
                    if (inputEvent.KeyEvent.KeyDown)
                    {
                        _keyDownHandler(new KeyEvent(map, _keyModifiers));
                        _keyHandler(new KeyEvent(map, _keyModifiers));
                    }
                    else
                    {
                        _keyUpHandler(new KeyEvent(map, _keyModifiers));
                    }
                    if (!inputEvent.KeyEvent.KeyDown)
                    {
                        _keyModifiers = null;
                    }
                }
                break;
            case EventType.Mouse:
                _mouseHandler(ToDriverMouse(inputEvent.MouseEvent));
                break;
            case EventType.Resize:
                this.TerminalSettings.WindowColumns = TerminalSettings.BufferColumns = inputEvent.ResizeEvent.Size.Width;
                this.TerminalSettings.WindowRows = TerminalSettings.BufferRows = inputEvent.ResizeEvent.Size.Height;
                ResizeScreen();
                UpdateOffScreen();
                TerminalResized?.Invoke();
                break;
        }
    }

    private MouseEvent ToDriverMouse(WebMouseEvent me)
    {
        MouseFlags mouseFlag = 0;

        if ((me.ButtonState & MouseButtonState.Button1Pressed) != 0)
        {
            mouseFlag |= MouseFlags.Button1Pressed;
        }
        if ((me.ButtonState & MouseButtonState.Button1Released) != 0)
        {
            mouseFlag |= MouseFlags.Button1Released;
        }
        if ((me.ButtonState & MouseButtonState.Button1Clicked) != 0)
        {
            mouseFlag |= MouseFlags.Button1Clicked;
        }
        if ((me.ButtonState & MouseButtonState.Button1DoubleClicked) != 0)
        {
            mouseFlag |= MouseFlags.Button1DoubleClicked;
        }
        if ((me.ButtonState & MouseButtonState.Button1TripleClicked) != 0)
        {
            mouseFlag |= MouseFlags.Button1TripleClicked;
        }
        if ((me.ButtonState & MouseButtonState.Button2Pressed) != 0)
        {
            mouseFlag |= MouseFlags.Button2Pressed;
        }
        if ((me.ButtonState & MouseButtonState.Button2Released) != 0)
        {
            mouseFlag |= MouseFlags.Button2Released;
        }
        if ((me.ButtonState & MouseButtonState.Button2Clicked) != 0)
        {
            mouseFlag |= MouseFlags.Button2Clicked;
        }
        if ((me.ButtonState & MouseButtonState.Button2DoubleClicked) != 0)
        {
            mouseFlag |= MouseFlags.Button2DoubleClicked;
        }
        if ((me.ButtonState & MouseButtonState.Button2TrippleClicked) != 0)
        {
            mouseFlag |= MouseFlags.Button2TripleClicked;
        }
        if ((me.ButtonState & MouseButtonState.Button3Pressed) != 0)
        {
            mouseFlag |= MouseFlags.Button3Pressed;
        }
        if ((me.ButtonState & MouseButtonState.Button3Released) != 0)
        {
            mouseFlag |= MouseFlags.Button3Released;
        }
        if ((me.ButtonState & MouseButtonState.Button3Clicked) != 0)
        {
            mouseFlag |= MouseFlags.Button3Clicked;
        }
        if ((me.ButtonState & MouseButtonState.Button3DoubleClicked) != 0)
        {
            mouseFlag |= MouseFlags.Button3DoubleClicked;
        }
        if ((me.ButtonState & MouseButtonState.Button3TripleClicked) != 0)
        {
            mouseFlag |= MouseFlags.Button3TripleClicked;
        }
        if ((me.ButtonState & MouseButtonState.ButtonWheeledUp) != 0)
        {
            mouseFlag |= MouseFlags.WheeledUp;
        }
        if ((me.ButtonState & MouseButtonState.ButtonWheeledDown) != 0)
        {
            mouseFlag |= MouseFlags.WheeledDown;
        }
        if ((me.ButtonState & MouseButtonState.ButtonWheeledLeft) != 0)
        {
            mouseFlag |= MouseFlags.WheeledLeft;
        }
        if ((me.ButtonState & MouseButtonState.ButtonWheeledRight) != 0)
        {
            mouseFlag |= MouseFlags.WheeledRight;
        }
        if ((me.ButtonState & MouseButtonState.Button4Pressed) != 0)
        {
            mouseFlag |= MouseFlags.Button4Pressed;
        }
        if ((me.ButtonState & MouseButtonState.Button4Released) != 0)
        {
            mouseFlag |= MouseFlags.Button4Released;
        }
        if ((me.ButtonState & MouseButtonState.Button4Clicked) != 0)
        {
            mouseFlag |= MouseFlags.Button4Clicked;
        }
        if ((me.ButtonState & MouseButtonState.Button4DoubleClicked) != 0)
        {
            mouseFlag |= MouseFlags.Button4DoubleClicked;
        }
        if ((me.ButtonState & MouseButtonState.Button4TripleClicked) != 0)
        {
            mouseFlag |= MouseFlags.Button4TripleClicked;
        }
        if ((me.ButtonState & MouseButtonState.ReportMousePosition) != 0)
        {
            mouseFlag |= MouseFlags.ReportMousePosition;
        }
        if ((me.ButtonState & MouseButtonState.ButtonShift) != 0)
        {
            mouseFlag |= MouseFlags.ButtonShift;
        }
        if ((me.ButtonState & MouseButtonState.ButtonCtrl) != 0)
        {
            mouseFlag |= MouseFlags.ButtonCtrl;
        }
        if ((me.ButtonState & MouseButtonState.ButtonAlt) != 0)
        {
            mouseFlag |= MouseFlags.ButtonAlt;
        }

        return new MouseEvent()
        {
            X = me.Position.X,
            Y = me.Position.Y,
            Flags = mouseFlag
        };
    }

    public override Attribute GetAttribute()
    {
        return this._currentAttribute;
    }

    /// <inheritdoc />
    public override bool GetCursorVisibility(out CursorVisibility visibility)
    {
        visibility = this.CursorVisible ? CursorVisibility.Default : CursorVisibility.Invisible;

        return false;
    }

    /// <inheritdoc />
    public override bool SetCursorVisibility(CursorVisibility visibility)
    {
        this.CursorVisible = visibility != CursorVisibility.Invisible;

        return false;
    }

    /// <inheritdoc />
    public override bool EnsureCursorVisibility()
    {
        return false;
    }

    public override void SendKeys(char keyChar, ConsoleKey key, bool shift, bool alt, bool control)
    {
        InputResult input = new InputResult();
        ConsoleKey ck;
        if (char.IsLetter(keyChar))
        {
            ck = key;
        }
        else
        {
            ck = (ConsoleKey)'\0';
        }
        input.EventType = EventType.Key;
        input.KeyEvent.ConsoleKeyInfo = new ConsoleKeyInfo(keyChar, ck, shift, alt, control);

        try
        {
            ProcessInput(input);
        }
        catch (OverflowException) { }
    }

    public void SetBufferSize(int width, int height)
    {
        this.TerminalSettings.BufferRows = height;
        this.TerminalSettings.BufferColumns = width;
        this.WindowRows = height;
        this.WindowColumns = width;
        this.ProcessResize();
    }

    public void SetWindowSize(int width, int height)
    {
        this.WindowColumns = width;
        this.WindowRows = height;
        if (width > this.BufferColumns || !this.HeightAsBuffer) this.BufferColumns = width;

        if (height > this.BufferRows || !this.HeightAsBuffer) this.BufferRows = height;

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

        this.Contents = new int[this.Rows, this.Cols, 3];
        this._dirtyLine = new bool[this.Rows];
    }

    private void UpdateOffScreen()
    {
        // Can raise an exception while is still resizing.
        try
        {
            for (var row = 0; row < this.Rows; row++)
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
                    this._dirtyLine[row] = true;
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
        // ReSharper disable HeapView.ObjectAllocation
        var values = Enum.GetValues(enumType: typeof(ConsoleColor)).OfType<ConsoleColor>()
            .Select(selector: s => (int)s);
        // ReSharper restore HeapView.ObjectAllocation
        var enumerable = values as int[] ?? values.ToArray();
        if (enumerable.Contains(value: value & 0xffff))
        {
            hasColor = true;
            background = (Color)(ConsoleColor)(value & 0xffff);
        }

        if (!enumerable.Contains(value: (value >> 16) & 0xffff)) return hasColor;
        hasColor = true;
        foreground = (Color)(ConsoleColor)((value >> 16) & 0xffff);

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