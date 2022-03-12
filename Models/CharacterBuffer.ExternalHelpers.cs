using System.Drawing;

namespace HACC.Models;

public partial class CharacterBuffer
{
    public bool BufferDirty
    {
        get
        {
            for (var y = 0; y < this.BufferRows; y++)
            {
                if (this.InternalBuffer[y].RowDirty)
                    return true;
            }

            return false;
        }
    }

    public bool BufferEffectsDirty
    {
        get
        {
            for (var y = 0; y < this.BufferRows; y++)
            {
                if (this.InternalBuffer[y].RowEffectsDirty)
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    ///     Retrieves a cloned copy of the private internal buffer.
    /// </summary>
    public BufferCharacter[,] BufferCharactersCopy
    {
        get
        {
            var newBuffer = new BufferCharacter[this.BufferColumns, this.BufferRows];
            for (var x = 0; x < this.BufferColumns; x++)
            for (var y = 0; y < this.BufferRows; y++)
            {
                newBuffer[x, y] = this.CharacterAt(
                        x: x,
                        y: y)
                    .Copy();
            }

            return newBuffer;
        }
    }

    public BufferRow[] BufferRowsCopy
    {
        get { return this.InternalBuffer.Select(selector: r => r.Copy()).ToArray(); }
    }

    /// <summary>
    ///     Gets the character at the specified position.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public BufferCharacter CharacterAt(int x, int y)
    {
        return this.InternalBuffer[y].RowCharacters[x].Copy();
    }

    /// <summary>
    ///     Gets the character at the specified position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public BufferCharacter CharacterAt(Point position)
    {
        return this.InternalBuffer[position.Y].RowCharacters[position.X].Copy();
    }

    public BufferRow CopyRow(int y)
    {
        return this.InternalBuffer[y].Copy();
    }
}