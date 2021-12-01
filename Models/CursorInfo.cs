using System.Drawing;
using HACC.Enumerations;

namespace HACC.Models;

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