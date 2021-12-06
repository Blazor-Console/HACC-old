namespace HACC.VirtualConsoleBuffer;

public struct BufferCharacter
{
    private string CharacterOriginal;
    private string CharacterState;
    private CharacterEffects CharacterEffectsOriginal;
    private CharacterEffects CharacterEffectsState;
    private bool ForceCharacterDirty;
    private bool ForceEffectsDirty;

    public string Character
    {
        get
        {
            return CharacterState;
        }
        internal set
        {
            CharacterState = new string(value);
        }
    }

    public bool CharacterChanged
    {
        get
        {
            return ForceCharacterDirty || CharacterState.Equals(CharacterOriginal);
        }
        internal set
        {
            ForceCharacterDirty = true;
        }
    }

    public CharacterEffects CharacterEffects
    {
        get
        {
            return CharacterEffectsState;
        }
        internal set
        {
            CharacterEffectsState = value.Copy();
        }
    }

    public bool CharacterEffectsChanged
    {
        get
        {
            return ForceEffectsDirty || CharacterEffectsState.Equals(CharacterEffectsOriginal);
        }
        internal set
        {
            ForceEffectsDirty = true;
        }
    }

    public BufferCharacter(string character, bool characterChanged, CharacterEffects characterEffects,
        bool characterEffectsChanged, string originalCharacterState = null, CharacterEffects? originalEffectsState = null)
    {
        CharacterOriginal = new string(string.IsNullOrEmpty(originalCharacterState) ? character : originalCharacterState);
        CharacterState = new string(character);
        ForceCharacterDirty = characterChanged;
        CharacterEffectsState = originalEffectsState is null ? characterEffects.Copy() : originalEffectsState.Value;
        CharacterEffectsOriginal = characterEffects.Copy();
        ForceEffectsDirty = characterEffectsChanged;
    }

    public BufferCharacter()
    {
        CharacterOriginal = string.Empty;
        CharacterState = string.Empty;
        ForceCharacterDirty = false;
        CharacterEffectsState = new CharacterEffects();
        CharacterEffectsOriginal = new CharacterEffects();
        ForceEffectsDirty = false;
    }

    public BufferCharacter Copy()
    {
        return new BufferCharacter(
            character: new string(Character),
            characterChanged: ForceCharacterDirty,
            characterEffects: CharacterEffects.Copy(),
            characterEffectsChanged: ForceEffectsDirty,
            originalCharacterState: new string(CharacterOriginal),
            originalEffectsState: CharacterEffectsOriginal.Copy());
    }

    public void AcceptChanges(bool character = true, bool effects = true)
    {
        if (character)
        {
            CharacterOriginal = CharacterState;
            ForceCharacterDirty = false;
        }
        if (effects)
        {
            CharacterEffectsOriginal = CharacterEffectsState;
            ForceEffectsDirty = false;
        }
    }

    public void RevertChanges(bool character = true, bool effects = true)
    {
        if (character)
        {
            CharacterState = CharacterOriginal;
            ForceCharacterDirty = false;
        }
        if (effects)
        {
            CharacterEffectsState = CharacterEffectsOriginal;
            ForceEffectsDirty = false;
        }
    }
}