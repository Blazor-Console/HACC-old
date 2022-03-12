using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using HACC.Enumerations;
using HACC.Models;
using HACC.Models.Drivers;
using HACC.Spectre;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;
using Terminal.Gui;

namespace HACC.Components;

public partial class Console
{
    private readonly ILogger _logger;
    public Console(ILogger logger)
    {
        this._logger = logger;
        this._canvasConsoleCore = new Html5AnsiConsoleCanvas(
            logger: this._logger ?? throw new InvalidOperationException(),
            console: this,
            terminalSettings: null);
    }

    private BECanvasComponent CanvasReference { get; set; } = null!;
    private Canvas2DContext _context = null!;
    private readonly Html5AnsiConsoleCanvas _canvasConsoleCore;

    protected new async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        this._context = await this.CanvasReference.CreateCanvas2DAsync();
        await _context.SetFillStyleAsync(value: "green");

        await _context.FillRectAsync(x: 10, y: 100, width: 100, height: 100);

        await _context.SetFontAsync(value: "48px serif");
        await _context.StrokeTextAsync(text: "Hello Blazor!!!", x: 10, y: 100);
    }

    public void RenderFullCharacterBuffer(CharacterBuffer characterBuffer)
    {
        characterBuffer.RenderFull(
            context: _context,
            canvas: this.CanvasReference);
    }

    public void RenderUpdatesFromCharacterBuffer(CharacterBuffer characterBuffer)
    {
        characterBuffer.RenderUpdates(
            context: _context,
            canvas: this.CanvasReference);
    }
}
