using System.Drawing;
using System.Globalization;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;

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

    /// <summary>
    ///     two-dimensional array of dirty state for whether the character itself has changed
    /// </summary>
    private readonly bool[,] CharacterChanged;

    /// <summary>
    ///     two-dimensional array of visual state for each character
    /// </summary>
    private readonly CharacterEffects[,] CharacterEffects;

    /// <summary>
    ///     two-dimensional array of dirty state for whether the character's appearance has changed
    /// </summary>
    private readonly bool[,] CharacterEffectsChanged;

    /// <summary>
    ///     two-dimensional array of "strings" to support multi-byte, containing the actual character buffer
    /// </summary>
    private readonly string[,] InternalBuffer;

    /// <summary>
    ///     Logging provider
    /// </summary>
    private readonly ILogger Logger;

    private Point CursorPosition;

    /// <summary>
    ///     Force next partial redraw to be a full redraw
    /// </summary>
    private bool ForceFullRender;

    public CharacterBuffer(ILogger logger, int columns, int rows)
    {
        Logger = logger;
        BufferColumns = columns;
        BufferRows = rows;
        InternalBuffer = new string[columns, rows];
        CharacterChanged = new bool[columns, rows];
        CharacterEffects = new CharacterEffects[columns, rows];
        CharacterEffectsChanged = new bool[columns, rows];
        ForceFullRender = true;
        CursorPosition = new Point(0, 0);
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                this.InternalBuffer[x, y] = "";
                this.CharacterChanged[x, y] = false;
                this.CharacterEffects[x, y] = new CharacterEffects(
                    bold: false,
                    italic: false,
                    underline: false,
                    inverse: false,
                    blink: false,
                    background: Defaults.BackgroundColor,
                    foreground: Defaults.ForegroundColor);
                this.CharacterChanged[x, y] = false;
            }
        }
    }

    /// <summary>
    ///     whether any of the appearance state is dirty
    /// </summary>
    public bool CharacterEffectsDirty { get; private set; }

    /// <summary>
    ///     whether any of the character state is dirty
    /// </summary>
    public bool CharacterBufferDirty { get; private set; }

    /// <summary>
    ///     Cursor can be moved by VT commands and/or can be used as a pointer/magnifier to give information about the
    ///     character at the current position.
    /// </summary>
    public CursorInfo Cursor =>
        new(
            new Point(
                CursorPosition.X,
                CursorPosition.Y),
            CharacterAt(CursorPosition),
            CharacterEffectsAt(CursorPosition));

    /// <summary>
    ///     Retrieves a cloned copy of the private internal buffer.
    /// </summary>
    public string[,] Buffer
    {
        get
        {
            var newBuffer = new string[BufferColumns, BufferRows];
            for (var x = 0; x < BufferColumns; x++)
            for (var y = 0; y < BufferRows; y++)
                newBuffer[x, y] = InternalBuffer[x, y];
            return newBuffer;
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
        var changed = !CharacterEffects[x, y].Equals(effects);
        CharacterEffects[x, y] = effects;
        CharacterEffectsChanged[x, y] = CharacterEffectsChanged[x, y] || changed;
        CharacterEffectsDirty = CharacterEffectsDirty || changed;
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
        {
            var changed = !CharacterEffects[x, y].Equals(effects);
            CharacterEffects[x, y] = effects;
            CharacterEffectsChanged[x, y] = CharacterEffectsChanged[x, y] || changed;
            CharacterEffectsDirty = CharacterEffectsDirty || changed;
        }
    }

    /// <summary>
    ///     Gets the character at the specified position.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public string CharacterAt(int x, int y)
    {
        return InternalBuffer[x, y];
    }

    /// <summary>
    ///     Gets the character at the specified position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public string CharacterAt(Point position)
    {
        return InternalBuffer[position.X, position.Y];
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
        if (x > BufferColumns || y > BufferRows)
            throw new ArgumentOutOfRangeException("x and y must be less than the buffer size");

        var length = GetLineElementCount(value, out var stringInfo);
        if (!string.IsNullOrEmpty(value))
            value = stringInfo.SubstringByTextElements(
                0,
                1);
        else if (stringInfo.LengthInTextElements == 0) value = "";

        var oldValue = InternalBuffer[x, y];
        var changed = !oldValue.Equals(value);
        InternalBuffer[x, y] = value;
        CharacterChanged[x, y] = CharacterChanged[x, y] || changed;
        CharacterBufferDirty = CharacterBufferDirty || changed;
        if (characterEffects.HasValue)
        {
            var effectsChanged = CharacterEffects[x, y].Equals(characterEffects.Value);
            CharacterEffects[x, y] = characterEffects.Value;
            CharacterEffectsChanged[x, y] = CharacterEffectsChanged[x, y] || effectsChanged;
            CharacterEffectsDirty = CharacterEffectsDirty || effectsChanged;
        }

        return oldValue;
    }

    /// <summary>
    ///     Gets the characters beginning at the specified coordinates, and extending up to the specified number of columns.
    ///     Setting length -1 will return all characters from the start column to the end of the buffer.
    /// <param name="x">X coordinate to write to</param>
    /// <param name="y">Y coordinate to write to</param>
    /// <param name="length">Maximum length- otherwise writes complete string until end of row.</param>
    /// </summary>
    public string GetLine(int x, int y, int length = -1)
    {
        var maxLength = BufferColumns - x;
        if (length < 0 || length > maxLength) length = maxLength;
        var substrings = new string[length];
        for (var i = 0; i < length; i++) substrings[i] = InternalBuffer[x + i, y];
        return string.Concat(substrings);
    }

    /// <summary>
    ///     Sets the characters beginning at the specified coordinates, and extending up to the specified number of columns.
    ///     Also applies character effects if specified.
    ///     The SetCharacter and SetLine methods are quasi-internal and bypass some of the cursor movement logic.
    /// <param name="x">X coordinate to write to</param>
    /// <param name="y">Y coordinate to write to</param>
    /// <param name="line">The line to write to the buffer.</param>
    /// <param name="length">Maximum length- otherwise writes complete string until end of row.</param>
    /// <param name="characterEffects">Optional character effects to apply.</param>
    /// </summary>
    public SetLineResponse SetLine(int x, int y, string line, int length = -1,
        CharacterEffects? characterEffects = null)
    {
        if (x > BufferColumns || y > BufferRows)
            throw new ArgumentOutOfRangeException("x and y must be less than the buffer size");

        var sourceLength = GetLineElementCount(line, out var sourceStringInfo);
        var maxLength = BufferColumns - x;
        if (length < 0 || length > maxLength) length = maxLength;
        if (length > sourceLength) length = sourceLength;

        var oldLine = GetLine(
            x,
            y,
            length);

        var oldStringInfo = new StringInfo(oldLine);
        for (var i = 0; i < length; i++)
        {
            var oldCharacter = string.IsNullOrEmpty(oldLine)
                ? ""
                : oldStringInfo.SubstringByTextElements(
                    i,
                    1);
            var newCharacter = sourceStringInfo.SubstringByTextElements(
                i,
                1);
            var changed = !oldCharacter.Equals(newCharacter);

            InternalBuffer[x + i, y] = newCharacter;
            CharacterChanged[x + i, y] =
                changed || CharacterChanged[x + i, y];
            CharacterBufferDirty = CharacterBufferDirty || changed;

            if (characterEffects.HasValue)
            {
                var effectsChanged = CharacterEffects[x + i, y].Equals(characterEffects.Value);
                CharacterEffects[x + i, y] = characterEffects.Value;
                CharacterEffectsChanged[x + i, y] = effectsChanged || CharacterEffectsChanged[x + i, y];
                CharacterEffectsDirty = effectsChanged || CharacterEffectsDirty;
            }
        }
        var remainder = sourceStringInfo.SubstringByTextElements(
            length,
            sourceLength - length);

        return new SetLineResponse(
            textReplaced: oldLine,
            lengthWritten: length,
            textOverflow: remainder);
    }

    /// <summary>
    ///     Writes a character to the buffer at the current cursor position and advances the cursor.
    ///     Processes only the first character (or multi-byte character) of the string.
    /// </summary>
    public void WriteChar(string character, CharacterEffects? characterEffects = null)
    {
        SetCharacter(
            CursorPosition.X,
            CursorPosition.Y,
            character,
            characterEffects);


        CursorPosition.X++;

        if (CursorPosition.X >= BufferColumns)
        {
            CursorPosition.X = 0;
            CursorPosition.Y++;
        }

        if (CursorPosition.Y >= BufferRows) CursorPosition.Y = 0;
    }

    /// <summary>
    ///     Returns the number of characters in the specified string, taking into account multi-byte characters.
    /// </summary>
    private int GetLineElementCount(string line, out StringInfo sourceStringInfo)
    {
        sourceStringInfo = new StringInfo(line);

        return sourceStringInfo.LengthInTextElements;
    }

    /// <summary>
    ///     Writes a string to the buffer at the specified coordinates and advances the cursor.
    ///     TODO: Handle control characters or disclaim.
    /// </summary>
    public void WriteLine(string? line, CharacterEffects? characterEffects = null, bool automaticNewLine = true, bool automaticWordWrap = false)
    {
        var setLineResponse = SetLine(
            CursorPosition.X,
            CursorPosition.Y,
            string.IsNullOrEmpty(line) ? string.Empty : line,
            characterEffects: characterEffects);
        
        // TODO: handle word wrap
        // determine if text printed exceeds the buffer width
        if (automaticWordWrap && !string.IsNullOrEmpty(setLineResponse.TextOverflow))
        {
            CursorPosition.X = 0;
            CursorPosition.Y++;
            WriteLine(
                line: setLineResponse.TextOverflow,
                characterEffects: characterEffects,
                automaticNewLine: automaticNewLine,
                automaticWordWrap: automaticWordWrap);
            return;
        }
        else if (automaticNewLine)
        {
            CursorPosition.X = 0;
            CursorPosition.Y++;
        }
        else
        {
            CursorPosition.X += setLineResponse.LengthWritten;
        }
    }

    /// <summary>
    ///     Gets the visual appearance of the character at the specified coordinates.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public CharacterEffects CharacterEffectsAt(int x, int y)
    {
        return CharacterEffects[x, y];
    }

    /// <summary>
    ///     Gets the visual appearance of the character at the specified coordinates.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public CharacterEffects CharacterEffectsAt(Point position)
    {
        return CharacterEffects[position.X, position.Y];
    }

    /// <summary>
    ///     whether the character at the given position has changed
    /// </summary>
    public bool CharacterDirty(int x, int y)
    {
        return CharacterChanged[x, y];
    }

    /// <summary>
    ///     whether the character effects at the given position have changed
    /// </summary>
    public bool EffectsDirty(int x, int y)
    {
        return CharacterEffectsChanged[x, y];
    }

    /// <summary>
    ///     Returns the coordinates of all section marked dirty.
    ///     When effects changes are included, also divides up ranges by changes to effects.
    /// </summary>
    public IEnumerable<DirtyRange> DirtyRanges(bool includeEffectsChanges = true)
    {
        var list = new List<DirtyRange>();
        for (var y = 0; y < BufferRows; y++)
        {
            var changeStart = -1;
            CharacterEffects? lastEffects = null;
            for (var x = 0; x < BufferColumns; x++)
            {
                var characterEffects = CharacterEffects[x, y];
                var characterChanged = CharacterChanged[x, y];
                var effectsDifferFromLastCharacter = includeEffectsChanges &&
                                                     (!lastEffects.HasValue || !lastEffects.Value.Equals(characterEffects));

                if (characterChanged && changeStart < 0) changeStart = x;

                if (changeStart >= 0 && x > changeStart && (!characterChanged || effectsDifferFromLastCharacter))
                {
                    // ended on the previous character
                    list.Add(new DirtyRange(
                        xStart: changeStart,
                        xEnd: x - 1,
                        y: y));
                    if (effectsDifferFromLastCharacter)
                    {
                        // change started this character
                        changeStart = x;
                    }
                    else
                    {
                        // change ended normally
                        changeStart = -1;
                    }
                }

                lastEffects = characterEffects;
            }

            // if still changes pending after completing line
            if (changeStart >= 0) list.Add(new DirtyRange(
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
        bool includeEffectsChanges = true)
    {
        var list = new List<DirtyRangeValue>();
        var ranges = DirtyRanges(includeEffectsChanges);
        foreach (var range in ranges)
        {
            var effectsForRange = CharacterEffects[range.XStart, range.Y];
            var rangeLength = range.XEnd - range.XStart + 1;

            list.Add(new DirtyRangeValue(
                xStart: range.XStart,
                xEnd: range.XEnd,
                y: range.Y,
                value: GetLine(
                    range.XStart,
                    range.Y,
                    rangeLength),
                characterEffects: effectsForRange));
        }

        return list;
    }

    /// <summary>
    ///     Returns a new buffer with the given dimension changes applied.
    ///     Lines will be truncated if the new area is smaller.
    /// </summary>
    public CharacterBuffer Resize(int newColumns, int newRows)
    {
        var newBuffer = new CharacterBuffer(
            Logger,
            newColumns,
            newRows);

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
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Renders only the changes since last render
    /// </summary>
    /// <param name="context"></param>
    /// <param name="canvas"></param>
    public void RenderUpdates(Canvas2DContext context, BECanvasComponent canvas)
    {
        Logger.LogInformation("Partial rendering requested");

        if (ForceFullRender)
        {
            Logger.LogInformation("Full render forced");
            RenderFull(
                context,
                canvas);
            ForceFullRender = false;
            return;
        }

        var dirtyRanges = DirtyRangeValues();
        throw new NotImplementedException();
    }
}