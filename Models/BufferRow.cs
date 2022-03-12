using HACC.Configuration;

namespace HACC.Models;

public struct BufferRow
{
    public readonly BufferCharacter[] RowCharacters;

    private BufferRow(IEnumerable<BufferCharacter> rowCharacters)
    {
        this.RowCharacters = rowCharacters.ToArray();
    }

    public BufferRow(int rowColumns)
    {
        this.RowCharacters = new BufferCharacter[rowColumns];
        this.RowCharacters.Initialize();
    }

    public BufferRow() : this(rowColumns: Defaults.InitialColumns)
    {
    }

    public BufferRow Copy()
    {
        return new BufferRow(rowCharacters: this.RowCharacters.Select(selector: c => c.Copy()).ToArray());
    }

    public bool RowDirty
    {
        get
        {
            for (var x = 0; x < this.RowCharacters.Length; x++)
            {
                if (this.RowCharacters[x].CharacterChanged)
                    return true;
            }

            return false;
        }
    }

    public bool RowEffectsDirty
    {
        get
        {
            for (var x = 0; x < this.RowCharacters.Length; x++)
            {
                if (this.RowCharacters[x].CharacterEffectsChanged)
                    return true;
            }

            return false;
        }
    }

    public void AcceptChanges(bool character = true, bool effects = true)
    {
        for (var x = 0; x < this.RowCharacters.Length; x++)
        {
            this.RowCharacters[x].AcceptChanges(character: character, effects: effects);
        }
    }

    public void RevertChanges(bool character = true, bool effects = true)
    {
        for (var x = 0; x < this.RowCharacters.Length; x++)
        {
            this.RowCharacters[x].RevertChanges(character: character, effects: effects);
        }
    }
}