using System.Globalization;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using HACC.Enumerations;
using HACC.Models;
using HACC.Models.Drivers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace HACC.Components;

public partial class Console : ComponentBase
{
    private static Dictionary<ConsoleType, Console> _singletons = new();
    private readonly Html5AnsiConsoleCanvas _canvasConsoleCore;
    private readonly ILogger _logger;
    private Canvas2DContext _context = null!;

    /// <summary>
    /// </summary>
    /// <param name="logger">dependency injected logger</param>
    /// <param name="webClipboard">dependency injected clipboard</param>
    /// <exception cref="InvalidOperationException"></exception>
    public Console(ILogger logger, WebClipboard webClipboard)
    {
        if (_singletons.ContainsKey(key: this.ConsoleType!.Value))
            throw new InvalidOperationException(message: "Console can only be instantiated once.");
        _singletons[key: this.ConsoleType!.Value] = this;
        this._logger = logger;
        this._canvasConsoleCore = new Html5AnsiConsoleCanvas(
            logger: this._logger ?? throw new InvalidOperationException(),
            console: this,
            terminalSettings: null,
            webClipboard: webClipboard);
    }

    [Parameter] public ConsoleType? ConsoleType { get; set; } = null!;

    private BECanvasComponent CanvasReference { get; set; } = null!;

    [Inject] private IJSRuntime jsRuntime { get; set; } = null!;

    public static Console GetInstance(ConsoleType consoleType)
    {
        if (_singletons.ContainsKey(key: consoleType)) return _singletons[key: consoleType];

        throw new ArgumentException(message: $"Console {consoleType} not found");
    }

    protected new async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender: firstRender);
        this._context = await this.CanvasReference.CreateCanvas2DAsync();
        await this._context.SetFillStyleAsync(value: "green");

        await this._context.FillRectAsync(x: 10, y: 100, width: 100, height: 100);

        await this._context.SetFontAsync(value: "48px serif");
        await this._context.StrokeTextAsync(text: "Hello Blazor!!!", x: 10, y: 100);
    }

    public void RenderFullCharacterBuffer(CharacterBuffer characterBuffer)
    {
        characterBuffer.RenderFull(
            context: this._context,
            canvas: this.CanvasReference);
    }

    public void RenderUpdatesFromCharacterBuffer(CharacterBuffer characterBuffer)
    {
        characterBuffer.RenderUpdates(
            context: this._context,
            canvas: this.CanvasReference);
    }

    /// <summary>
    ///     Invoke the javascript beep function (copied from JavasScript/beep.js)
    /// </summary>
    /// <param name="duration">duration of the tone in milliseconds. Default is 500</param>
    /// <param name="frequency">frequency of the tone in hertz. default is 440</param>
    /// <param name="volume">volume of the tone. Default is 1, off is 0.</param>
    /// <param name="type">type of tone. Possible values are sine, square, sawtooth, triangle, and custom. Default is sine.</param>
    public async Task Beep(float? duration, float? frequency, float? volume, string? type)
    {
        if (duration is not null && frequency is not null && volume is not null && type is not null)
            await this.jsRuntime.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.InvariantCulture),
                volume.Value.ToString(provider: CultureInfo.InvariantCulture),
                type);
        if (duration is not null && frequency is not null && volume is not null && type is null)
            await this.jsRuntime.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.CurrentCulture),
                volume.Value.ToString(provider: CultureInfo.InvariantCulture));
        if (duration is not null && frequency is not null && volume is null && type is null)
            await this.jsRuntime.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.InvariantCulture));
        if (duration is not null && frequency is null && volume is null && type is null)
            await this.jsRuntime.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.CurrentCulture));
        if (duration is null && frequency is null && volume is null && type is null)
            await this.jsRuntime.InvokeVoidAsync(
                identifier: "beep");
    }

    private async Task OnCanvasClick(MouseEventArgs obj)
    {
        throw new NotImplementedException();
    }

    private async Task OnCanvasKeyDown(KeyboardEventArgs obj)
    {
        throw new NotImplementedException();
    }

    private async Task OnCanvasKeyUp(KeyboardEventArgs obj)
    {
        throw new NotImplementedException();
    }

    private async Task OnCanvasKeyPress(KeyboardEventArgs arg)
    {
        throw new NotImplementedException();
    }
}