using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using HACC.VirtualConsoleBuffer;

namespace HACC.Components;

public partial class Console
{
    protected BECanvasComponent _canvasReference;
    private Canvas2DContext _context;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _context = await _canvasReference.CreateCanvas2DAsync();
        await _context.SetFillStyleAsync("green");

        await _context.FillRectAsync(10, 100, 100, 100);

        await _context.SetFontAsync("48px serif");
        await _context.StrokeTextAsync("Hello Blazor!!!", 10, 100);
    }

    public void RenderFullCharacterBuffer(CharacterBuffer characterBuffer)
    {
        characterBuffer.RenderFull(
            _context,
            _canvasReference);
    }

    public void RenderUpdatesFromCharacterBuffer(CharacterBuffer characterBuffer)
    {
        characterBuffer.RenderUpdates(
            _context,
            _canvasReference);
    }
}