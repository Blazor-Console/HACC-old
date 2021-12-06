using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using System.Drawing;
using System.Globalization;

namespace HACC.VirtualConsoleBuffer;

public class CharacterBuffer
{
    /// <summary>
    ///     The number of columns in the buffer
    /// </summary>
    private readonly int BufferColumns;

    /// <summary>
    ///     The number of rows in the buffer
    /// </summary>
    private readonly int BufferRows;

    private readonly BufferRow[] InternalBuffer;

    /// <summary>
    ///     Logging provider
    /// </summary>
    private readonly ILogger Logger;

    private Point CursorPosition;
    public CursorType CursorType { get; internal set; }

    /// <summary>
    ///     Force next partial redraw to be a full redraw
    /// </summary>
    private bool ForceFullRender;

    public CharacterBuffer(ILogger logger, int columns = Defaults.InitialColumns, int rows = Defaults.InitialRows, CursorType cursorType = Defaults.CursorShape)
    {
        Logger = logger;
        BufferColumns = columns;
        BufferRows = rows;
        InternalBuffer = new BufferRow[rows];
        ForceFullRender = true;
        CursorPosition = new Point(x: 0, y: 0);
        for (var y = 0; y < rows; y++)
            InternalBuffer[y] = new BufferRow(rowColumns: columns);
        CursorType = cursorType;
    }

    public CharacterBuffer()
    {
        throw new NotImplementedException();
    }

    public bool BufferDirty
    {
        get
        {
            for (int y = 0; y < BufferRows; y++)
            {
                if (InternalBuffer[y].RowDirty)
                    return true;
            }
            return false;
        }
    }

    public bool BufferEffectsDirty
    {
        get
        {
            for (int y = 0; y < BufferRows; y++)
            {
                if (InternalBuffer[y].RowEffectsDirty)
                    return true;
            }
            return false;
        }
    }


    /// <summary>
    ///     Cursor can be moved by VT commands and/or can be used as a pointer/magnifier to give information about the
    ///     character at the current position.
    /// </summary>
    public CursorInfo Cursor =>
        new(
            position: new Point(
                x: CursorPosition.X,
                y: CursorPosition.Y),
            character: CharacterAt(position: CursorPosition),
            type: CursorType);

    /// <summary>
    ///     Retrieves a cloned copy of the private internal buffer.
    /// </summary>
    public BufferCharacter[,] BufferCharactersCopy
    {
        get
        {
            var newBuffer = new BufferCharacter[BufferColumns, BufferRows];
            for (var x = 0; x < BufferColumns; x++)
                for (var y = 0; y < BufferRows; y++)
                    newBuffer[x, y] = this
                        .CharacterAt(
                            x: x,
                            y: y)
                        .Copy();
            return newBuffer;
        }
    }

    public BufferRow[] BufferRowsCopy
    {
        get
        {
            return InternalBuffer.Select(r => r.Copy()).ToArray();
        }
    }

    /// <summary>
    ///     Sets the visual appearance of the character at the specified position.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="effects"></param>
    public void SetCharacterEffects(int x, int y, CharacterEffects effects)
    {
        InternalBuffer[y].RowCharacters[x].CharacterEffects = effects.Copy();
    }

    public void SetCharacterEffects(Point position, CharacterEffects effects)
    {
        SetCharacterEffects(
            x: position.X,
            y: position.Y,
            effects: effects);
    }

    /// <summary>
    ///     Sets the visual appearance of the character at the specified position.
    /// </summary>
    /// <param name="xStart"></param>
    /// <param name="xEnd"></param>
    /// <param name="y"></param>
    /// <param name="effects"></param>
    public void SetCharacterEffects(int xStart, int xEnd, int y, CharacterEffects effects)
    {
        for (var x = xStart; x <= xEnd; x++)
            SetCharacterEffects(
                x: x,
                y: y,
                effects: effects);
    }

    /// <summary>
    ///     Gets the character at the specified position.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public BufferCharacter CharacterAt(int x, int y)
    {
        return InternalBuffer[y].RowCharacters[x].Copy();
    }

    /// <summary>
    ///     Gets the character at the specified position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public BufferCharacter CharacterAt(Point position)
    {
        return InternalBuffer[position.Y].RowCharacters[position.X].Copy();
    }

    /// <summary>
    ///     Sets the character at the specified position.
    ///     Processes only the first character (or multi-byte character) of the string.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="value"></param>
    /// <param name="characterEffects"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public string SetCharacter(int x, int y, string value, CharacterEffects? characterEffects = null)
    {
        if (x >= BufferColumns || y >= BufferRows)
            throw new ArgumentException(message: "x and y must be less than the buffer size");
        if (x < 0)
            throw new ArgumentOutOfRangeException(paramName: nameof(x));
        if (y < 0)
            throw new ArgumentOutOfRangeException(paramName: nameof(y));

        var length = GetLineElementCount(
            line: value,
            sourceStringInfo: out var stringInfo);

        value = !string.IsNullOrEmpty(value: value) && (stringInfo.LengthInTextElements == 0) ? stringInfo.SubstringByTextElements(
            startingTextElement: 0,
            lengthInTextElements: 1) : string.Empty;

        string oldValue = new string(InternalBuffer[y].RowCharacters[x].Character);
        InternalBuffer[y].RowCharacters[x].Character = new string(value);
        if (characterEffects.HasValue)
            SetCharacterEffects(
                x: x,
                y: y,
                effects: characterEffects.Value);

        return oldValue;
    }

    public string SetCharacter(Point position, string value, CharacterEffects? characterEffects = null)
    {
        return SetCharacter(
            x: position.X,
            y: position.Y,
            value: value,
            characterEffects: characterEffects);
    }

    /// <summary>
    ///     Gets the characters beginning at the specified coordinates, and extending up to the specified number of columns.
    ///     Setting length -1 will return all characters from the start column to the end of the buffer.
    ///     <param name="x">X coordinate to write to</param>
    ///     <param name="y">Y coordinate to write to</param>
    ///     <param name="length">Maximum length- otherwise writes complete string until end of row.</param>
    /// </summary>
    public string GetLine(int x, int y, int length = -1)
    {
        var maxLength = BufferColumns - x;
        if (length < 0 || length > maxLength) length = maxLength;
        var substrings = new string[length];
        for (var i = 0; i < length; i++) substrings[i] = new string(InternalBuffer[y].RowCharacters[x + i].Character);
        return string.Concat(values: substrings);
    }

    public string GetLine(Point position, int length = -1)
    {
        return GetLine(
            x: position.X,
            y: position.Y,
            length: length);
    }

    /// <summary>
    ///     Sets the characters beginning at the specified coordinates, and extending up to the specified number of columns.
    ///     Also applies character effects if specified.
    ///     The SetCharacter and SetLine methods are quasi-internal and bypass some of the cursor movement logic.
    ///     <param name="x">X coordinate to write to</param>
    ///     <param name="y">Y coordinate to write to</param>
    ///     <param name="line">The line to write to the buffer.</param>
    ///     <param name="length">Maximum length- otherwise writes complete string until end of row.</param>
    ///     <param name="characterEffects">Optional character effects to apply.</param>
    /// </summary>
    public SetLineResponse SetLine(int x, int y, string line, int length = -1,
        CharacterEffects? characterEffects = null)
    {
        if (x >= BufferColumns || y >= BufferRows)
            throw new ArgumentException(message: "x and y must be less than the buffer size");
        if (x < 0)
            throw new ArgumentOutOfRangeException(paramName: nameof(x));
        if (y < 0)
            throw new ArgumentOutOfRangeException(paramName: nameof(y));

        var sourceLength = GetLineElementCount(line: line, sourceStringInfo: out var sourceStringInfo);
        var maxLength = BufferColumns - x;
        if (length < 0 || length > maxLength) length = maxLength;
        if (length > sourceLength) length = sourceLength;

        var oldLine = GetLine(
            x: x,
            y: y,
            length: length);

        for (var i = 0; i < length; i++)
        {
            var newCharacter = sourceStringInfo.SubstringByTextElements(
                startingTextElement: i,
                lengthInTextElements: 1);
            SetCharacter(
                x: x + i,
                y: y,
                value: newCharacter,
                characterEffects: characterEffects);
        }

        var remaining = sourceLength - length;
        var remainder = remaining <= 0 ? string.Empty : sourceStringInfo.SubstringByTextElements(
            startingTextElement: length,
            lengthInTextElements: remaining);

        return new SetLineResponse(
            textReplaced: oldLine,
            lengthWritten: length,
            textOverflow: remainder);
    }

    public SetLineResponse SetLine(Point position, string line, int length = -1)
    {
        return SetLine(
            x: position.X,
            y: position.Y,
            line: line,
            length: length);
    }

    public BufferRow CopyRow(int y)
    {
        return InternalBuffer[y].Copy();
    }

    public BufferRow[] ScrollLine(int count = 1)
    {
        if (count < 1 || count > BufferRows) throw new ArgumentOutOfRangeException(paramName: nameof(count));

        var removedLines = new BufferRow[count];
        var startingRow = BufferRows - count - 1;

        // Copy the lines to be removed
        for (int y = startingRow; y < BufferRows; y++)
        {
            removedLines[y] = InternalBuffer[y].Copy();
        }
        // move down the lines above the starting row
        for (int y = startingRow - count; y > count; y--)
        {
            InternalBuffer[y] = InternalBuffer[y - count];
        }
        // replace the top N empty rows
        for (int y = 0; y < count; y++)
        {
            InternalBuffer[y] = new BufferRow(rowColumns: BufferColumns);
        }

        return removedLines;
    }

    /// <summary>
    ///     Writes a character to the buffer at the current cursor position and advances the cursor.
    ///     Processes only the first character (or multi-byte character) of the string.
    /// </summary>
    public void WriteChar(string character, CharacterEffects? characterEffects = null)
    {
        SetCharacter(
            x: CursorPosition.X,
            y: CursorPosition.Y,
            value: character,
            characterEffects: characterEffects);


        CursorPosition.X++;

        if (CursorPosition.X >= BufferColumns)
        {
            CursorPosition.X = 0;
            CursorPosition.Y++;
        }

        // TODO: implement scroll
        if (CursorPosition.Y >= BufferRows) CursorPosition.Y = 0;
    }

    /// <summary>
    ///     Returns the number of characters in the specified string, taking into account multi-byte characters.
    /// </summary>
    private int GetLineElementCount(string line, out StringInfo sourceStringInfo)
    {
        sourceStringInfo = new StringInfo(value: string.IsNullOrEmpty(line) ? String.Empty : line);

        return sourceStringInfo.LengthInTextElements;
    }

    /// <summary>
    ///     Writes a string to the buffer at the specified coordinates and advances the cursor.
    ///     TODO: Handle control characters or disclaim.
    /// </summary>
    public void WriteLine(string? line, CharacterEffects? characterEffects = null, bool automaticNewLine = true,
        bool automaticWordWrap = false)
    {
        var setLineResponse = SetLine(
            x: CursorPosition.X,
            y: CursorPosition.Y,
            line: string.IsNullOrEmpty(value: line) ? string.Empty : line,
            characterEffects: characterEffects);

        if (automaticWordWrap && !string.IsNullOrEmpty(value: setLineResponse.TextOverflow))
        {
            Logger.LogDebug(message: "Automatic word wrap");
            CursorPosition.X = 0;
            CursorPosition.Y++;
            WriteLine(
                line: setLineResponse.TextOverflow,
                characterEffects: characterEffects,
                automaticNewLine: automaticNewLine,
                automaticWordWrap: automaticWordWrap);
            return;
        }

        if (automaticNewLine)
        {
            CursorPosition.X = 0;
            CursorPosition.Y++;
        }
        else
        {
            CursorPosition.X += setLineResponse.LengthWritten;
        }

        // TODO: implement scroll
        if (CursorPosition.Y >= BufferRows) CursorPosition.Y = 0;
    }

    /// <summary>
    ///     Gets the visual appearance of the character at the specified coordinates.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public CharacterEffects CharacterEffectsAt(int x, int y)
    {
        return InternalBuffer[y].RowCharacters[x].CharacterEffects.Copy();
    }

    /// <summary>
    ///     Gets the visual appearance of the character at the specified coordinates.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public CharacterEffects CharacterEffectsAt(Point position)
    {
        return InternalBuffer[position.Y].RowCharacters[position.X].CharacterEffects.Copy();
    }

    /// <summary>
    ///     whether the character at the given position has changed
    /// </summary>
    public bool CharacterDirtyAt(int x, int y)
    {
        return InternalBuffer[y].RowCharacters[x].CharacterChanged;
    }

    public bool CharacterDirtyAt(Point position)
    {
        return InternalBuffer[position.Y].RowCharacters[position.X].CharacterChanged;
    }

    /// <summary>
    ///     whether the character effects at the given position have changed
    /// </summary>
    public bool CharacterEffectsDirtyAt(int x, int y)
    {
        return InternalBuffer[y].RowCharacters[x].CharacterEffectsChanged;
    }

    public bool CharacterEffectsDirtyAt(Point position)
    {
        return InternalBuffer[position.Y].RowCharacters[position.X].CharacterEffectsChanged;
    }

    /// <summary>
    ///     Returns the coordinates of all section marked dirty.
    ///     When effects changes are included, also divides up ranges by changes to effects.
    /// </summary>
    public IEnumerable<DirtyRange> DirtyRanges(bool includeCharacterChanges = true, bool includeEffectsChanges = true)
    {
        var list = new List<DirtyRange>();
        for (var y = 0; y < BufferRows; y++)
        {
            var changeStart = -1;
            CharacterEffects? lastEffects = null;

            // Break into sections beginning with 

            for (var x = 0; x < BufferColumns; x++)
            {
                if (changeStart < 0)
                {
                    bool startChange = false;
                    if (includeCharacterChanges && InternalBuffer[y].RowCharacters[x].CharacterChanged)
                    {
                        startChange = true;
                    }
                    if (includeEffectsChanges)
                    {
                        // if either dirty or differing from effects style being printed, break into section
                        if (!InternalBuffer[y].RowCharacters[x].CharacterEffects.Equals(lastEffects) || InternalBuffer[y].RowCharacters[x].CharacterEffectsChanged)
                            startChange = true;

                    }
                    if (startChange)
                        changeStart = x;
                }
                // if change started and advanced past change
                if ((changeStart >= 0) && (x > changeStart))
                {
                    var charactersChanged = includeCharacterChanges &&
                        InternalBuffer[y].RowCharacters[x].CharacterChanged;

                    var effectsChangedFromLast = includeEffectsChanges && !lastEffects.Equals(InternalBuffer[y].RowCharacters[x].CharacterEffects);
                    var effectsChanged = includeEffectsChanges && InternalBuffer[y].RowCharacters[x].CharacterEffectsChanged;

                    if (!charactersChanged && !effectsChangedFromLast && !effectsChanged)
                    {
                        list.Add(item: new DirtyRange(
                            xStart: changeStart,
                            xEnd: x,
                            y: y));

                        // change ended
                        changeStart = -1;
                    }
                }

                lastEffects = InternalBuffer[y].RowCharacters[x].CharacterEffects.Copy();
            }

            // if still changes pending after completing line
            if (changeStart >= 0)
                list.Add(item: new DirtyRange(
                    xStart: changeStart,
                    xEnd: BufferColumns - 1,
                    y: y));
        }

        return list;
    }

    /// <summary>
    ///     Returns a collection of ranges marked dirty and their corresponding text and character effects
    /// </summary>
    public IEnumerable<DirtyRangeValue> DirtyRangeValues(
        bool includeCharacterChanges = true,
        bool includeEffectsChanges = true)
    {
        return DirtyRanges(
            includeCharacterChanges: includeCharacterChanges,
            includeEffectsChanges: includeEffectsChanges).Select(range => new DirtyRangeValue(
                xStart: range.XStart,
                xEnd: range.XEnd,
                y: range.Y,
                value: GetLine(
                    x: range.XStart,
                    y: range.Y,
                    length: range.XEnd - range.XStart + 1),
                characterEffects: InternalBuffer[range.Y].RowCharacters[range.XStart].CharacterEffects.Copy()))
            .ToArray();
    }

    /// <summary>
    ///     Returns a new buffer with the given dimension changes applied.
    ///     Lines will be truncated if the new area is smaller.
    /// </summary>
    public CharacterBuffer Resize(int newColumns, int newRows)
    {
        var newBuffer = new CharacterBuffer(
            logger: Logger,
            columns: newColumns,
            rows: newRows);

        // TODO: copy
        throw new NotImplementedException();

        return newBuffer;
    }

    /// <summary>
    ///     Redraws the entire canvas
    /// </summary>
    /// <param name="context"></param>
    /// <param name="canvas"></param>
    public void RenderFull(Canvas2DContext context, BECanvasComponent canvas)
    {
        AcceptChanges();
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Renders only the changes since last render
    /// </summary>
    /// <param name="context"></param>
    /// <param name="canvas"></param>
    public void RenderUpdates(Canvas2DContext context, BECanvasComponent canvas)
    {
        Logger.LogInformation(message: "Partial rendering requested");

        if (ForceFullRender)
        {
            Logger.LogInformation(message: "Full render forced");
            RenderFull(
                context: context,
                canvas: canvas);
            ForceFullRender = false;
            return;
        }

        var dirtyRanges = DirtyRangeValues();
        AcceptChanges();
        throw new NotImplementedException();
    }

    public void AcceptChanges(bool character = true, bool effects = true)
    {
        for (int y = 0; y < BufferRows; y++)
        {
            InternalBuffer[y].AcceptChanges(character: character, effects: effects);
        }
    }

    public void RevertChanges(bool character = true, bool effects = true)
    {
        for (int y = 0; y < BufferRows; y++)
        {
            InternalBuffer[y].RevertChanges(character: character, effects: effects);
        }
    }
}