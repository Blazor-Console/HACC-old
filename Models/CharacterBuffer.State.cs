using System.Drawing;

namespace HACC.Models;

public partial class CharacterBuffer
{
    /// <summary>
    ///     whether the character at the given position has changed
    /// </summary>
    public bool CharacterDirtyAt(int x, int y)
    {
        return InternalBuffer[y].RowCharacters[x].CharacterChanged;
    }

    public bool CharacterDirtyAt(Point position)
    {
        return InternalBuffer[position.Y].RowCharacters[position.X].CharacterChanged;
    }

    /// <summary>
    ///     Returns the coordinates of all section marked dirty.
    ///     When effects changes are included, also divides up ranges by changes to effects.
    /// </summary>
    public IEnumerable<DirtyRange> DirtyRanges(bool includeCharacterChanges = true, bool includeEffectsChanges = true)
    {
        var list = new List<DirtyRange>();
        for (var y = 0; y < BufferRows; y++)
        {
            var changeStart = -1;
            CharacterEffects? lastEffects = null;

            // Break into sections beginning with 

            for (var x = 0; x < BufferColumns; x++)
            {
                if (changeStart < 0)
                {
                    var startChange = false;
                    if (includeCharacterChanges && InternalBuffer[y].RowCharacters[x].CharacterChanged)
                        startChange = true;
                    if (includeEffectsChanges)
                        // if either dirty or differing from effects style being printed, break into section
                        if (lastEffects != null && (!InternalBuffer[y].RowCharacters[x].CharacterEffects.Equals(obj: lastEffects) ||
                                                    InternalBuffer[y].RowCharacters[x].CharacterEffectsChanged))
                            startChange = true;
                    if (startChange)
                        changeStart = x;
                }

                // if change started and advanced past change
                if (changeStart >= 0 && x > changeStart)
                {
                    var charactersChanged = includeCharacterChanges &&
                                            InternalBuffer[y].RowCharacters[x].CharacterChanged;

                    var effectsChangedFromLast = includeEffectsChanges &&
                                                 !lastEffects.Equals(other: InternalBuffer[y].RowCharacters[x]
                                                     .CharacterEffects);
                    var effectsChanged = includeEffectsChanges &&
                                         InternalBuffer[y].RowCharacters[x].CharacterEffectsChanged;

                    if (!charactersChanged && !effectsChangedFromLast && !effectsChanged)
                    {
                        list.Add(item: new DirtyRange(
                            xStart: changeStart,
                            xEnd: x,
                            y: y));

                        // change ended
                        changeStart = -1;
                    }
                }

                lastEffects = InternalBuffer[y].RowCharacters[x].CharacterEffects.Copy();
            }

            // if still changes pending after completing line
            if (changeStart >= 0)
                list.Add(item: new DirtyRange(
                    xStart: changeStart,
                    xEnd: BufferColumns - 1,
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
        return DirtyRanges(
                includeCharacterChanges: includeCharacterChanges,
                includeEffectsChanges: includeEffectsChanges).Select(selector: range => new DirtyRangeValue(
                xStart: range.XStart,
                xEnd: range.XEnd,
                y: range.Y,
                value: GetLine(
                    x: range.XStart,
                    y: range.Y,
                    length: range.XEnd - range.XStart + 1),
                characterEffects: InternalBuffer[range.Y].RowCharacters[range.XStart].CharacterEffects.Copy()))
            .ToArray();
    }

    public void AcceptChanges(bool character = true, bool effects = true)
    {
        for (var y = 0; y < BufferRows; y++) InternalBuffer[y].AcceptChanges(character: character, effects: effects);
    }

    public void RevertChanges(bool character = true, bool effects = true)
    {
        for (var y = 0; y < BufferRows; y++) InternalBuffer[y].RevertChanges(character: character, effects: effects);
    }
}
