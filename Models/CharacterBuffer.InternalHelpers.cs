using System.Drawing;
using System.Globalization;

namespace HACC.Models;

public partial class CharacterBuffer
{
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
        if (x >= this.BufferColumns || y >= this.BufferRows)
            throw new ArgumentException(message: "x and y must be less than the buffer size");
        if (x < 0)
            throw new ArgumentOutOfRangeException(paramName: nameof(x));
        if (y < 0)
            throw new ArgumentOutOfRangeException(paramName: nameof(y));

        var length = this.GetLineElementCount(
            line: value,
            sourceStringInfo: out var stringInfo);

        value = !string.IsNullOrEmpty(value: value) && stringInfo.LengthInTextElements == 0
            ? stringInfo.SubstringByTextElements(
                startingTextElement: 0,
                lengthInTextElements: 1)
            : string.Empty;

        var oldValue = new string(value: this.InternalBuffer[y].RowCharacters[x].Character);
        this.InternalBuffer[y].RowCharacters[x].Character = new string(value: value);
        if (characterEffects.HasValue)
            this.SetCharacterEffects(
                x: x,
                y: y,
                effects: characterEffects.Value);

        return oldValue;
    }

    public string SetCharacter(Point position, string value, CharacterEffects? characterEffects = null)
    {
        return this.SetCharacter(
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
        var maxLength = this.BufferColumns - x;
        if (length < 0 || length > maxLength) length = maxLength;
        var substrings = new string[length];
        for (var i = 0; i < length; i++)
        {
            substrings[i] = new string(value: this.InternalBuffer[y].RowCharacters[x + i].Character);
        }

        return string.Concat(values: substrings);
    }

    public string GetLine(Point position, int length = -1)
    {
        return this.GetLine(
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
        if (x >= this.BufferColumns || y >= this.BufferRows)
            throw new ArgumentException(message: "x and y must be less than the buffer size");
        if (x < 0)
            throw new ArgumentOutOfRangeException(paramName: nameof(x));
        if (y < 0)
            throw new ArgumentOutOfRangeException(paramName: nameof(y));

        var sourceLength = this.GetLineElementCount(line: line, sourceStringInfo: out var sourceStringInfo);
        var maxLength = this.BufferColumns - x;
        if (length < 0 || length > maxLength) length = maxLength;
        if (length > sourceLength) length = sourceLength;

        var oldLine = this.GetLine(
            x: x,
            y: y,
            length: length);

        for (var i = 0; i < length; i++)
        {
            var newCharacter = sourceStringInfo.SubstringByTextElements(
                startingTextElement: i,
                lengthInTextElements: 1);
            this.SetCharacter(
                x: x + i,
                y: y,
                value: newCharacter,
                characterEffects: characterEffects);
        }

        var remaining = sourceLength - length;
        var remainder = remaining <= 0
            ? string.Empty
            : sourceStringInfo.SubstringByTextElements(
                startingTextElement: length,
                lengthInTextElements: remaining);

        return new SetLineResponse(
            textReplaced: oldLine,
            lengthWritten: length,
            textOverflow: remainder);
    }

    public SetLineResponse SetLine(Point position, string line, int length = -1)
    {
        return this.SetLine(
            x: position.X,
            y: position.Y,
            line: line,
            length: length);
    }


    public BufferRow[] ScrollLine(int count = 1)
    {
        if (count < 1 || count > this.BufferRows) throw new ArgumentOutOfRangeException(paramName: nameof(count));

        var removedLines = new BufferRow[count];
        var startingRow = this.BufferRows - count - 1;

        // Copy the lines to be removed
        for (var y = startingRow; y < this.BufferRows; y++)
        {
            removedLines[y] = this.CopyRow(y: y);
        }

        // move down the lines above the starting row
        for (var y = startingRow - count; y > count; y--)
        {
            this.InternalBuffer[y] = this.InternalBuffer[y - count];
        }

        // replace the top N empty rows
        for (var y = 0; y < count; y++)
        {
            this.InternalBuffer[y] = new BufferRow(rowColumns: this.BufferColumns);
        }

        return removedLines;
    }

    /// <summary>
    ///     Returns the number of characters in the specified string, taking into account multi-byte characters.
    /// </summary>
    private int GetLineElementCount(string line, out StringInfo sourceStringInfo)
    {
        sourceStringInfo = new StringInfo(value: string.IsNullOrEmpty(value: line) ? string.Empty : line);

        return sourceStringInfo.LengthInTextElements;
    }
}