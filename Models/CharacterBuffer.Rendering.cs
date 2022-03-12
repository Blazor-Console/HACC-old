using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.Extensions.Logging;

namespace HACC.Models;

public partial class CharacterBuffer
{
    public void Clear(bool clearCharacters = true, bool clearEffects = true)
    {
        for (var x = 0; x < this.BufferColumns; x++)
        for (var y = 0; y < this.BufferRows; y++)
        {
            var character = this.InternalBuffer[y].RowCharacters[x];
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
            logger: this.Logger,
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
        this.AcceptChanges();
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Renders only the changes since last render
    /// </summary>
    /// <param name="context"></param>
    /// <param name="canvas"></param>
    public void RenderUpdates(Canvas2DContext context, BECanvasComponent canvas)
    {
        this.Logger.LogInformation(message: "Partial rendering requested");

        if (this.ForceFullRender)
        {
            this.Logger.LogInformation(message: "Full render forced");
            this.RenderFull(
                context: context,
                canvas: canvas);
            this.ForceFullRender = false;
            return;
        }

        var dirtyRanges = this.DirtyRangeValues();
        this.AcceptChanges();
        throw new NotImplementedException();
    }
}