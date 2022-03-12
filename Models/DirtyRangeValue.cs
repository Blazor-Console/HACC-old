namespace HACC.Models;

public struct DirtyRangeValue
{
    public readonly int XStart;
    public readonly int XEnd;
    public readonly int Y;
    public readonly string Value;
    public readonly CharacterEffects CharacterEffects;

    public DirtyRangeValue(int xStart, int xEnd, int y, string value, CharacterEffects characterEffects)
    {
        this.XStart = xStart;
        this.XEnd = xEnd;
        this.Y = y;
        this.Value = value;
        this.CharacterEffects = characterEffects;
    }
}