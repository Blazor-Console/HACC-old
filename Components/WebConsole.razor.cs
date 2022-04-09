using System.Drawing;
using System.Globalization;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using HACC.Applications;
using HACC.Extensions;
using HACC.Models;
using HACC.Models.Drivers;
using HACC.Models.Enums;
using HACC.Models.Structs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace HACC.Components;

public partial class WebConsole : ComponentBase
{
    private static readonly IJSRuntime JsInterop = HaccExtensions.GetService<IJSRuntime>();
    private static readonly ILogger Logger = HaccExtensions.CreateLogger<WebConsole>();

    private readonly Dictionary<Rune, TextMetrics> MeasuredRunes = new();
    private readonly Dictionary<string, TextMetrics> MeasuredText = new();

    /// <summary>
    ///     Null until after render
    /// </summary>
    private BECanvasComponent? _beCanvas;

    /// <summary>
    ///     Null until after render when we initialize it from the beCanvas reference
    /// </summary>
    private Canvas2DContext? _canvas2DContext;

    /// <summary>
    ///     Null until after render
    /// </summary>
    private ElementReference? _divCanvas;

    private Queue<InputResult> _inputResultQueue = new();
    private int _screenHeight = 480;
    private int _screenWidth = 640;

    public WebApplication? WebApplication { get; private set; }

    public WebConsoleDriver? WebConsoleDriver { get; private set; }

    public WebMainLoopDriver? WebMainLoopDriver { get; private set; }

    public bool CanvasInitialized => this._canvas2DContext is { };

    [Parameter] public EventCallback OnLoaded { get; set; }

    public event Action<InputResult> ReadConsoleInput;
    public event Action RunIterationNeeded;

    protected override Task OnInitializedAsync()
    {
        this.WebConsoleDriver = new WebConsoleDriver(
            webClipboard: HaccExtensions.WebClipboard,
            webConsole: this);
        this.WebMainLoopDriver = new WebMainLoopDriver(webConsole: this);
        this.WebApplication = new WebApplication(
            webConsoleDriver: this.WebConsoleDriver,
            webMainLoopDriver: this.WebMainLoopDriver,
            webConsole: this);

        return base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Logger.LogDebug(message: "OnAfterRenderAsync");
            this._canvas2DContext = await this._beCanvas.CreateCanvas2DAsync();

            var thisObject = DotNetObjectReference.Create(value: this);
            await JsInterop!.InvokeVoidAsync(identifier: "initConsole",
                thisObject);
            // this will make sure that the viewport is correctly initialized
            await JsInterop!.InvokeAsync<object>(identifier: "consoleWindowResize",
                thisObject);
            await JsInterop!.InvokeAsync<object>(identifier: "consoleWindowFocus",
                thisObject);
            await JsInterop!.InvokeAsync<object>(identifier: "consoleWindowBeforeUnload",
                thisObject);

            await this.OnLoaded.InvokeAsync();

            Logger.LogDebug(message: "OnAfterRenderAsync: end");
        }

        await base.OnAfterRenderAsync(firstRender: firstRender);
    }

    public async Task<object?> DrawBufferToPng()
    {
        if (!this.CanvasInitialized) return null;
        return await JsInterop!.InvokeAsync<object>(identifier: "canvasToPng");
    }

    public async Task<TextMetrics?> MeasureRune(Rune rune)
    {
        if (!this.CanvasInitialized) return null;
        if (this.MeasuredRunes.ContainsKey(key: rune))
            return this.MeasuredRunes[key: rune];

        var runeString = rune.ToString();
        var runeStringRef = DotNetObjectReference.Create(value: runeString);
        var result = await JsInterop!.InvokeAsync<object>(identifier: "canvasMeasureText",
            runeStringRef);
        var textMetrics = (TextMetrics) result;
        this.MeasuredRunes.Add(
            key: rune,
            value: textMetrics);
        return textMetrics;
    }

    public async Task<TextMetrics?> MeasureText(string text)
    {
        if (!this.CanvasInitialized) return null;
        if (this.MeasuredText.ContainsKey(key: text))
            return this.MeasuredText[key: text];

        var totalWidth = 0;
        var maxHeight = -1;
        foreach (var ch in text)
        {
            var measuredRune = await this.MeasureRune(rune: new Rune(ch: ch));
            if (measuredRune is null) continue;
            totalWidth += measuredRune.width;
            if (maxHeight < measuredRune.height)
                maxHeight = measuredRune.height;
        }

        var textRef = DotNetObjectReference.Create(value: text);
        var result = await JsInterop!.InvokeAsync<object>(identifier: "canvasMeasureText",
            textRef);

        var textMetrics = (TextMetrics)result;

        if (totalWidth != textMetrics.width)
            Logger.LogDebug(message: "totalWidth ({totalWidth}) != result.width ({result.width})",
                totalWidth,
                textMetrics.width);
        this.MeasuredText.Add(
            key: text,
            value: textMetrics);
        return textMetrics;
    }

    private async Task RedrawCanvas()
    {
        if (!this.CanvasInitialized) return;

        Logger.LogDebug(message: "InitializeNewCanvasFrame");

        // TODO: actually clear the canvas
        await this._canvas2DContext!.SetFillStyleAsync(value: "blue");
        await this._canvas2DContext.ClearRectAsync(
            x: 0,
            y: 0,
            width: this.WebConsoleDriver!.WindowWidthPixels,
            height: this.WebConsoleDriver.WindowHeightPixels);
        await this._canvas2DContext.FillRectAsync(
            x: 0,
            y: 0,
            width: this.WebConsoleDriver.WindowWidthPixels,
            height: this.WebConsoleDriver.WindowHeightPixels);


        //await this._canvas2DContextStdErr.SetFillStyleAsync(value: "blue");
        //await this._canvas2DContextStdErr.ClearRectAsync(
        //    x: 0,
        //    y: 0,
        //    width: this._webConsoleDriver.WindowWidthPixels,
        //    height: this._webConsoleDriver.WindowHeightPixels);
        //await this._canvas2DContextStdErr.FillRectAsync(
        //    x: 0,
        //    y: 0,
        //    width: this._webConsoleDriver.WindowWidthPixels,
        //    height: this._webConsoleDriver.WindowHeightPixels);
        Logger.LogDebug(message: "InitializeNewCanvasFrame: end");
    }

    public async Task DrawDirtySegmentToCanvas((int attribute, int row, int col, string text) segment,
        TerminalSettings terminalSettings)
    {
        if (!this.CanvasInitialized) return;
        Logger.LogDebug(message: "DrawBufferToFrame");
        var colorFound = this.WebConsoleDriver!.GetColors(
            value: segment.attribute,
            foreground: out var foreground,
            background: out var background);

        if (!colorFound)
        {
            Logger.LogDebug(message: $"Color not found for attribute {segment.attribute}");
            return;
        }

        var measuredText = await this.MeasureText(text: segment.text);
        var measuredRune = await this.MeasureRune(rune: new Rune(segment.text[0]));

        //if (firstRender.HasValue && firstRender.Value || this._canvas2DContext is null)
        //    await this.RedrawCanvas();
        await this._canvas2DContext!.SetFontAsync(
            value: $"{measuredRune!.width}px " +
                   $"{terminalSettings.FontType}");
        await this._canvas2DContext.SetTextBaselineAsync(value: TextBaseline.Top);
        await this._canvas2DContext!.SetFillStyleAsync(
            value: $"{background}");
        await this._canvas2DContext.FillRectAsync(
            x: segment.col * measuredRune.width,
            y: segment.row * measuredRune.height,
            width: measuredText!.width,
            height: measuredText.height);
        await this._canvas2DContext!.SetStrokeStyleAsync(
            value: $"{foreground}");
        await this._canvas2DContext.StrokeTextAsync(text: segment.text,
            x: segment.col,
            y: segment.row);
        Logger.LogDebug(message: "DrawBufferToFrame: end");
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
            // ReSharper disable HeapView.ObjectAllocation
            await JsInterop.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.InvariantCulture),
                volume.Value.ToString(provider: CultureInfo.InvariantCulture),
                type);
        if (duration is not null && frequency is not null && volume is not null && type is null)
            await JsInterop.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.CurrentCulture),
                volume.Value.ToString(provider: CultureInfo.InvariantCulture));
        if (duration is not null && frequency is not null && volume is null && type is null)
            await JsInterop.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.InvariantCulture));
        if (duration is not null && frequency is null && volume is null && type is null)
            await JsInterop.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.CurrentCulture));
        if (duration is null && frequency is null && volume is null && type is null)
            await JsInterop.InvokeVoidAsync(
                identifier: "beep");
        // ReSharper restore HeapView.ObjectAllocation
    }

    private void OnReadConsoleInput(InputResult inputResult)
    {
        this.ReadConsoleInput?.Invoke(obj: inputResult);
        this.RunIterationNeeded?.Invoke();
    }

    [JSInvokable]
    public async Task OnCanvasClick(MouseEventArgs obj)
    {
        // of relevance: ActiveConsole
        var inputResult = new InputResult
        {
            EventType = EventType.Mouse,
            MouseEvent = new WebMouseEvent
            {
                ButtonState = MouseButtonState.Button1Clicked,
            },
        };
        this.OnReadConsoleInput(inputResult: inputResult);
    }

    [JSInvokable]
    public async Task OnCanvasKeyDown(KeyboardEventArgs obj)
    {
        // of relevance: ActiveConsole
        throw new NotImplementedException();
    }

    [JSInvokable]
    public async Task OnCanvasKeyUp(KeyboardEventArgs obj)
    {
        // of relevance: ActiveConsole
        throw new NotImplementedException();
    }

    [JSInvokable]
    public async Task OnCanvasKeyPress(KeyboardEventArgs arg)
    {
        // of relevance: ActiveConsole
        throw new NotImplementedException();
    }

    [JSInvokable]
    public async ValueTask OnResize(int screenWidth, int screenHeight)
    {
        if (this._canvas2DContext == null) return;
        this._screenWidth = screenWidth;
        this._screenHeight = screenHeight;
        var inputResult = new InputResult
        {
            EventType = EventType.Resize,
            ResizeEvent = new ResizeEvent
            {
                Size = new Size(width: screenWidth,
                    height: screenHeight),
            },
        };
        this.OnReadConsoleInput(inputResult: inputResult);
    }

    [JSInvokable]
    public async ValueTask OnFocus()
    {
        if (this._canvas2DContext == null) return;
    }

    [JSInvokable]
    public async ValueTask OnBeforeUnload()
    {
        if (this._canvas2DContext == null) return;
    }
}
