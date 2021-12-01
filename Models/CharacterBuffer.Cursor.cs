using System.Drawing;
using HACC.Enumerations;

namespace HACC.Models;

public partial class CharacterBuffer
{
    private Point CursorPosition;

    public CursorType CursorType { get; internal set; }


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
}