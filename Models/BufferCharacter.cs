namespace HACC.Models;

public struct BufferCharacter
{
    /// <summary>
    ///     Committed state
    /// </summary>
    private string CharacterOriginal;

    /// <summary>
    ///     Current state
    /// </summary>
    private string CharacterState;

    private CharacterEffects CharacterEffectsOriginal;
    private CharacterEffects CharacterEffectsState;
    private bool ForceCharacterDirty;
    private bool ForceEffectsDirty;

    public string Character
    {
        get => this.CharacterState;
        internal set => this.CharacterState = new string(value: value);
    }

    public bool CharacterChanged
    {
        get => this.ForceCharacterDirty || !this.CharacterState.Equals(value: this.CharacterOriginal);
        internal set => this.ForceCharacterDirty = true;
    }

    public CharacterEffects CharacterEffects
    {
        get => this.CharacterEffectsState;
        internal set => this.CharacterEffectsState = value.Copy();
    }

    public bool CharacterEffectsChanged
    {
        get => this.ForceEffectsDirty || !this.CharacterEffectsState.Equals(other: this.CharacterEffectsOriginal);
        internal set => this.ForceEffectsDirty = true;
    }

    public BufferCharacter(string character, bool characterChanged, CharacterEffects characterEffects,
        bool characterEffectsChanged, string? originalCharacterState = null,
        CharacterEffects? originalEffectsState = null)
    {
        this.CharacterOriginal =
            new string(value: string.IsNullOrEmpty(value: originalCharacterState) ? character : originalCharacterState);
        this.CharacterState = new string(value: character);
        this.ForceCharacterDirty = characterChanged;
        // ReSharper disable once MergeConditionalExpression
        this.CharacterEffectsState =
            originalEffectsState.HasValue ? originalEffectsState.Value : characterEffects.Copy();
        this.CharacterEffectsOriginal = characterEffects.Copy();
        this.ForceEffectsDirty = characterEffectsChanged;
    }

    public BufferCharacter()
    {
        this.CharacterOriginal = string.Empty;
        this.CharacterState = string.Empty;
        this.ForceCharacterDirty = false;
        this.CharacterEffectsState = new CharacterEffects();
        this.CharacterEffectsOriginal = new CharacterEffects();
        this.ForceEffectsDirty = false;
    }

    public BufferCharacter Copy()
    {
        return new BufferCharacter(
            character: new string(value: this.Character),
            characterChanged: this.ForceCharacterDirty,
            characterEffects: this.CharacterEffects.Copy(),
            characterEffectsChanged: this.ForceEffectsDirty,
            originalCharacterState: new string(value: this.CharacterOriginal),
            originalEffectsState: this.CharacterEffectsOriginal.Copy());
    }

    public void AcceptChanges(bool character = true, bool effects = true)
    {
        if (character)
        {
            this.CharacterOriginal = this.CharacterState;
            this.ForceCharacterDirty = false;
        }

        if (effects)
        {
            this.CharacterEffectsOriginal = this.CharacterEffectsState;
            this.ForceEffectsDirty = false;
        }
    }

    public void RevertChanges(bool character = true, bool effects = true)
    {
        if (character)
        {
            this.CharacterState = this.CharacterOriginal;
            this.ForceCharacterDirty = false;
        }

        if (effects)
        {
            this.CharacterEffectsState = this.CharacterEffectsOriginal;
            this.ForceEffectsDirty = false;
        }
    }
}