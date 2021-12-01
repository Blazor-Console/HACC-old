using System.Drawing;

namespace HACC.Models;

public partial class CharacterBuffer
{
    public bool BufferDirty
    {
        get
        {
            for (var y = 0; y < BufferRows; y++)
                if (InternalBuffer[y].RowDirty)
                    return true;
            return false;
        }
    }

    public bool BufferEffectsDirty
    {
        get
        {
            for (var y = 0; y < BufferRows; y++)
                if (InternalBuffer[y].RowEffectsDirty)
                    return true;
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
            var newBuffer = new BufferCharacter[BufferColumns, BufferRows];
            for (var x = 0; x < BufferColumns; x++)
            for (var y = 0; y < BufferRows; y++)
                newBuffer[x, y] =
                    CharacterAt(
                            x: x,
                            y: y)
                        .Copy();
            return newBuffer;
        }
    }

    public BufferRow[] BufferRowsCopy
    {
        get { return InternalBuffer.Select(selector: r => r.Copy()).ToArray(); }
    }

    /// <summary>
    ///     Gets the character at the specified position.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public BufferCharacter CharacterAt(int x, int y)
    {
        return InternalBuffer[y].RowCharacters[x].Copy();
    }

    /// <summary>
    ///     Gets the character at the specified position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public BufferCharacter CharacterAt(Point position)
    {
        return InternalBuffer[position.Y].RowCharacters[position.X].Copy();
    }

    public BufferRow CopyRow(int y)
    {
        return InternalBuffer[y].Copy();
    }
}