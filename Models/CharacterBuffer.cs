using System.Drawing;
using HACC.Configuration;
using HACC.Enumerations;
using Microsoft.Extensions.Logging;

namespace HACC.Models;

public partial class CharacterBuffer
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

    /// <summary>
    ///     Force next partial redraw to be a full redraw
    /// </summary>
    private bool ForceFullRender;

    public CharacterBuffer(ILogger logger, int columns = Defaults.InitialColumns, int rows = Defaults.InitialRows,
        CursorType cursorType = Defaults.CursorShape)
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
        Logger.LogDebug(message: string.Format(format: "Created virtual buffer with {0}x{1} rows and columns",
            arg0: rows, arg1: columns));
    }

    public CharacterBuffer()
    {
        throw new NotImplementedException();
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
        //ScrollLine()
        if (CursorPosition.Y >= BufferRows) CursorPosition.Y = 0;
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
        //ScrollLine()
        if (CursorPosition.Y >= BufferRows) CursorPosition.Y = 0;
    }
}