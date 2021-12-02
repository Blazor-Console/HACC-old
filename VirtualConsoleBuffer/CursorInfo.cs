using System.Drawing;

namespace HACC.VirtualConsoleBuffer;

public struct CursorInfo
{
    public readonly Point Position;
    public readonly string Character;
    public readonly CharacterEffects CharacterEffects;

    public CursorInfo(Point position, string character, CharacterEffects characterEffects)
    {
        Position = position;
        Character = character;
        CharacterEffects = characterEffects;
    }
}