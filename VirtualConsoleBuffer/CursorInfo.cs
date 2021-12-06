using System.Drawing;

namespace HACC.VirtualConsoleBuffer;

public struct CursorInfo
{
    public readonly Point Position;
    public readonly BufferCharacter Character;
    public readonly CursorType Type;

    public CursorInfo(Point position, BufferCharacter character, CursorType type)
    {
        Position = position;
        Character = character;
        Type = type;
    }
}