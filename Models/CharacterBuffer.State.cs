using System.Drawing;

namespace HACC.Models;

public partial class CharacterBuffer
{
    /// <summary>
    ///     whether the character at the given position has changed
    /// </summary>
    public bool CharacterDirtyAt(int x, int y)
    {
        return this.InternalBuffer[y].RowCharacters[x].CharacterChanged;
    }

    public bool CharacterDirtyAt(Point position)
    {
        return this.InternalBuffer[position.Y].RowCharacters[position.X].CharacterChanged;
    }

    /// <summary>
    ///     Returns the coordinates of all section marked dirty.
    ///     When effects changes are included, also divides up ranges by changes to effects.
    /// </summary>
    public IEnumerable<DirtyRange> DirtyRanges(bool includeCharacterChanges = true, bool includeEffectsChanges = true)
    {
        var list = new List<DirtyRange>();
        for (var y = 0; y < this.BufferRows; y++)
        {
            var rangeStartIndex = -1;
            CharacterEffects? lastEffects = null;

            // Break into sections beginning with 

            for (var x = 0; x < this.BufferColumns; x++)
            {
                if (rangeStartIndex < 0)
                {
                    var changed = false;
                    if (includeCharacterChanges && this.InternalBuffer[y].RowCharacters[x].CharacterChanged)
                        changed = true;
                    if (includeEffectsChanges)
                        // if either dirty or differing from effects style being printed, break into section
                        if (lastEffects != null &&
                            (!this.InternalBuffer[y].RowCharacters[x].CharacterEffects.Equals(obj: lastEffects) ||
                             this.InternalBuffer[y].RowCharacters[x].CharacterEffectsChanged))
                            changed = true;
                    if (changed)
                        rangeStartIndex = x;
                }

                // if change started and advanced past change
                if (rangeStartIndex >= 0 && x > rangeStartIndex)
                {
                    var charactersChanged = includeCharacterChanges &&
                                            this.InternalBuffer[y].RowCharacters[x].CharacterChanged;

                    var effectsChangedFromLast = includeEffectsChanges &&
                                                 !lastEffects.Equals(other: this.InternalBuffer[y].RowCharacters[x]
                                                     .CharacterEffects);
                    var effectsChanged = includeEffectsChanges &&
                                         this.InternalBuffer[y].RowCharacters[x].CharacterEffectsChanged;

                    if (!charactersChanged && !effectsChangedFromLast && !effectsChanged)
                    {
                        list.Add(item: new DirtyRange(
                            xStart: rangeStartIndex,
                            xEnd: x,
                            y: y));

                        // change ended
                        rangeStartIndex = -1;
                    }
                }

                lastEffects = this.InternalBuffer[y].RowCharacters[x].CharacterEffects.Copy();
            }

            // if still changes pending after completing line
            if (rangeStartIndex >= 0)
                list.Add(item: new DirtyRange(
                    xStart: rangeStartIndex,
                    xEnd: this.BufferColumns - 1,
                    y: y));
        }

        return list;
    }

    /// <summary>
    ///     Returns a collection of ranges marked dirty and their corresponding text and character effects
    /// </summary>
    public IEnumerable<DirtyRangeValue> DirtyRangeValues(
        bool includeCharacterChanges = true,
        bool includeEffectsChanges = true)
    {
        return this.DirtyRanges(
                includeCharacterChanges: includeCharacterChanges,
                includeEffectsChanges: includeEffectsChanges).Select(selector: range => new DirtyRangeValue(
                xStart: range.XStart,
                xEnd: range.XEnd,
                y: range.Y,
                value: this.GetLine(
                    x: range.XStart,
                    y: range.Y,
                    length: range.XEnd - range.XStart + 1),
                characterEffects: this.InternalBuffer[range.Y].RowCharacters[range.XStart].CharacterEffects.Copy()))
            .ToArray();
    }

    public void AcceptChanges(bool character = true, bool effects = true)
    {
        for (var y = 0; y < this.BufferRows; y++)
        {
            this.InternalBuffer[y].AcceptChanges(character: character, effects: effects);
        }
    }

    public void RevertChanges(bool character = true, bool effects = true)
    {
        for (var y = 0; y < this.BufferRows; y++)
        {
            this.InternalBuffer[y].RevertChanges(character: character, effects: effects);
        }
    }
}