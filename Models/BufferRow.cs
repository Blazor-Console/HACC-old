using HACC.Configuration;

namespace HACC.Models;

public struct BufferRow
{
    public readonly BufferCharacter[] RowCharacters;

    private BufferRow(IEnumerable<BufferCharacter> rowCharacters)
    {
        RowCharacters = rowCharacters.ToArray();
    }

    public BufferRow(int rowColumns)
    {
        RowCharacters = new BufferCharacter[rowColumns];
        RowCharacters.Initialize();
    }

    public BufferRow() : this(rowColumns: Defaults.InitialColumns)
    {
    }

    public BufferRow Copy()
    {
        return new BufferRow(rowCharacters: RowCharacters.Select(selector: c => c.Copy()).ToArray());
    }

    public bool RowDirty
    {
        get
        {
            for (var x = 0; x < RowCharacters.Length; x++)
                if (RowCharacters[x].CharacterChanged)
                    return true;
            return false;
        }
    }

    public bool RowEffectsDirty
    {
        get
        {
            for (var x = 0; x < RowCharacters.Length; x++)
                if (RowCharacters[x].CharacterEffectsChanged)
                    return true;
            return false;
        }
    }

    public void AcceptChanges(bool character = true, bool effects = true)
    {
        for (var x = 0; x < RowCharacters.Length; x++)
            RowCharacters[x].AcceptChanges(character: character, effects: effects);
    }

    public void RevertChanges(bool character = true, bool effects = true)
    {
        for (var x = 0; x < RowCharacters.Length; x++)
            RowCharacters[x].RevertChanges(character: character, effects: effects);
    }
}