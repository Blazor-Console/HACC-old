namespace HACC.VirtualConsoleBuffer;

public struct RowCopy
{
    public readonly string[] Buffer;
    public readonly bool[] CharacterChanged;
    public readonly CharacterEffects[] CharacterEffects;
    public readonly bool[] CharacterEffectsChanged;

    public RowCopy(string[] buffer, bool[] characterChanged, CharacterEffects[] characterEffects,
        bool[] characterEffectsChanged)
    {
        Buffer = buffer;
        CharacterChanged = characterChanged;
        CharacterEffects = characterEffects;
        CharacterEffectsChanged = characterEffectsChanged;
    }
}