using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.Extensions.Logging;

namespace HACC.Models;

public partial class CharacterBuffer
{
    public void Clear(bool clearCharacters = true, bool clearEffects = true)
    {
        for (var x = 0; x < BufferColumns; x++)
        for (var y = 0; y < BufferRows; y++)
        {
            var character = InternalBuffer[y].RowCharacters[x];
            if (clearCharacters) character.Character = string.Empty;
            if (clearEffects) character.CharacterEffects = new CharacterEffects();

            character.AcceptChanges(
                character: clearCharacters,
                effects: clearEffects);
        }
    }

    /// <summary>
    ///     Returns a new buffer with the given dimension changes applied.
    ///     Lines will be truncated if the new area is smaller.
    /// </summary>
    public CharacterBuffer Resize(int newColumns, int newRows)
    {
        var newBuffer = new CharacterBuffer(
            logger: Logger,
            columns: newColumns,
            rows: newRows);

        // TODO: copy
        throw new NotImplementedException();

        return newBuffer;
    }

    /// <summary>
    ///     Redraws the entire canvas
    /// </summary>
    /// <param name="context"></param>
    /// <param name="canvas"></param>
    public void RenderFull(Canvas2DContext context, BECanvasComponent canvas)
    {
        AcceptChanges();
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Renders only the changes since last render
    /// </summary>
    /// <param name="context"></param>
    /// <param name="canvas"></param>
    public void RenderUpdates(Canvas2DContext context, BECanvasComponent canvas)
    {
        Logger.LogInformation(message: "Partial rendering requested");

        if (ForceFullRender)
        {
            Logger.LogInformation(message: "Full render forced");
            RenderFull(
                context: context,
                canvas: canvas);
            ForceFullRender = false;
            return;
        }

        var dirtyRanges = DirtyRangeValues();
        AcceptChanges();
        throw new NotImplementedException();
    }
}