namespace HACC.Models;

public struct BufferCharacter
{
    /// <summary>
    /// Committed state
    /// </summary>
    private string CharacterOriginal;

    /// <summary>
    /// Current state
    /// </summary>
    private string CharacterState;
    private CharacterEffects CharacterEffectsOriginal;
    private CharacterEffects CharacterEffectsState;
    private bool ForceCharacterDirty;
    private bool ForceEffectsDirty;

    public string Character
    {
        get => CharacterState;
        internal set => CharacterState = new string(value: value);
    }

    public bool CharacterChanged
    {
        get => ForceCharacterDirty || !CharacterState.Equals(value: CharacterOriginal);
        internal set => ForceCharacterDirty = true;
    }

    public CharacterEffects CharacterEffects
    {
        get => CharacterEffectsState;
        internal set => CharacterEffectsState = value.Copy();
    }

    public bool CharacterEffectsChanged
    {
        get => ForceEffectsDirty || !CharacterEffectsState.Equals(other: CharacterEffectsOriginal);
        internal set => ForceEffectsDirty = true;
    }

    public BufferCharacter(string character, bool characterChanged, CharacterEffects characterEffects,
        bool characterEffectsChanged, string? originalCharacterState = null,
        CharacterEffects? originalEffectsState = null)
    {
        CharacterOriginal =
            new string(value: string.IsNullOrEmpty(value: originalCharacterState) ? character : originalCharacterState);
        CharacterState = new string(value: character);
        ForceCharacterDirty = characterChanged;
        // ReSharper disable once MergeConditionalExpression
        CharacterEffectsState = originalEffectsState.HasValue ? originalEffectsState.Value : characterEffects.Copy(); 
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
            character: new string(value: Character),
            characterChanged: ForceCharacterDirty,
            characterEffects: CharacterEffects.Copy(),
            characterEffectsChanged: ForceEffectsDirty,
            originalCharacterState: new string(value: CharacterOriginal),
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
