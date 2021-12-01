using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using HACC.Models;

namespace HACC.Components;

public partial class Console
{
    protected BECanvasComponent _canvasReference;
    private Canvas2DContext _context;

    protected async Task OnAfterRenderAsync(bool firstRender)
    {
        _context = await _canvasReference.CreateCanvas2DAsync();
        await _context.SetFillStyleAsync(value: "green");

        await _context.FillRectAsync(x: 10, y: 100, width: 100, height: 100);

        await _context.SetFontAsync(value: "48px serif");
        await _context.StrokeTextAsync(text: "Hello Blazor!!!", x: 10, y: 100);
    }

    public void RenderFullCharacterBuffer(CharacterBuffer characterBuffer)
    {
        characterBuffer.RenderFull(
            context: _context,
            canvas: _canvasReference);
    }

    public void RenderUpdatesFromCharacterBuffer(CharacterBuffer characterBuffer)
    {
        characterBuffer.RenderUpdates(
            context: _context,
            canvas: _canvasReference);
    }
}