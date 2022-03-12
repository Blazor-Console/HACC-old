using System.Drawing;

namespace HACC.Models;

public partial class CharacterBuffer
{
    /// <summary>
    ///     Sets the visual appearance of the character at the specified position.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="effects"></param>
    public void SetCharacterEffects(int x, int y, CharacterEffects effects)
    {
        this.InternalBuffer[y].RowCharacters[x].CharacterEffects = effects.Copy();
    }

    public void SetCharacterEffects(Point position, CharacterEffects effects)
    {
        this.SetCharacterEffects(
            x: position.X,
            y: position.Y,
            effects: effects);
    }

    /// <summary>
    ///     Sets the visual appearance of the character at the specified position.
    /// </summary>
    /// <param name="xStart"></param>
    /// <param name="xEnd"></param>
    /// <param name="y"></param>
    /// <param name="effects"></param>
    public void SetCharacterEffects(int xStart, int xEnd, int y, CharacterEffects effects)
    {
        for (var x = xStart; x <= xEnd; x++)
        {
            this.SetCharacterEffects(
                x: x,
                y: y,
                effects: effects);
        }
    }

    /// <summary>
    ///     Gets the visual appearance of the character at the specified coordinates.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public CharacterEffects CharacterEffectsAt(int x, int y)
    {
        return this.InternalBuffer[y].RowCharacters[x].CharacterEffects.Copy();
    }

    /// <summary>
    ///     Gets the visual appearance of the character at the specified coordinates.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public CharacterEffects CharacterEffectsAt(Point position)
    {
        return this.InternalBuffer[position.Y].RowCharacters[position.X].CharacterEffects.Copy();
    }

    /// <summary>
    ///     whether the character effects at the given position have changed
    /// </summary>
    public bool CharacterEffectsDirtyAt(int x, int y)
    {
        return this.InternalBuffer[y].RowCharacters[x].CharacterEffectsChanged;
    }

    public bool CharacterEffectsDirtyAt(Point position)
    {
        return this.InternalBuffer[position.Y].RowCharacters[position.X].CharacterEffectsChanged;
    }
}