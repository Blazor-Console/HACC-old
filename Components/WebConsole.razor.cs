using System.Globalization;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using HACC.Applications;
using HACC.Extensions;
using HACC.Models;
using HACC.Models.Drivers;
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
    private int screenWidth = 640;
    private int screenHeight = 480;

    /// <summary>
    /// Null until after render
    /// </summary>
    private BECanvasComponent? _beCanvas;

    /// <summary>
    /// Null until after render when we initialize it from the beCanvas reference
    /// </summary>
    private Canvas2DContext? _canvas2DContext;

    /// <summary>
    /// Null until after render
    /// </summary>
    private ElementReference? _divCanvas;

    public WebApplication? WebApplication { get; private set; }

    public WebConsoleDriver? WebConsoleDriver { get; private set; }

    public WebMainLoopDriver? WebMainLoopDriver { get; private set; }

    public bool CanvasInitialized => this._canvas2DContext is { };

    private Queue<InputResult> _inputResultQueue = new Queue<InputResult>();

    [Parameter]
    public EventCallback OnLoaded { get; set; }

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
            _canvas2DContext = await _beCanvas.CreateCanvas2DAsync();
            await _canvas2DContext.SetTextBaselineAsync(TextBaseline.Top);

            var thisObject = DotNetObjectReference.Create(this);
            await JsInterop!.InvokeVoidAsync("initConsole", thisObject);
            // this will make sure that the viewport is correctly initialized
            await JsInterop!.InvokeAsync<object>("consoleWindowResize", thisObject);
            await JsInterop!.InvokeAsync<object>("consoleWindowFocus", thisObject);
            await JsInterop!.InvokeAsync<object>("consoleWindowBeforeUnload", thisObject);

            await this.OnLoaded.InvokeAsync();

            Logger.LogDebug(message: "OnAfterRenderAsync: end");
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    public async Task<object?> DrawBufferToPng()
    {
        if (!this.CanvasInitialized)
        {
            return null;
        }
        return await JsInterop!.InvokeAsync<object>(identifier: "canvasToPng");
    }

    private async Task RedrawCanvas()
    {
        if (!this.CanvasInitialized)
        {
            return;
        }

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

    public async Task DrawUpdatesToCanvas(string output, double x, double y)
    {
        if (!this.CanvasInitialized)
        {
            return;
        }
        Logger.LogDebug(message: "DrawBufferToFrame");
        //if (firstRender.HasValue && firstRender.Value || this._canvas2DContext is null)
        //    await this.RedrawCanvas();
        await this._canvas2DContext!.SetFillStyleAsync(value: $"{WebConsoleDriver!.TerminalSettings.TerminalBackground}");
        await this._canvas2DContext.ClearRectAsync(
            x: x,
            y: y,
            width: output.Length,
            height: this.WebConsoleDriver.WindowHeightPixels);
        await this._canvas2DContext!.SetStrokeStyleAsync(value: $"{WebConsoleDriver!.TerminalSettings.TerminalForeground}");
        await this._canvas2DContext!.SetFontAsync(
            value: $"{WebConsoleDriver!.TerminalSettings.FontSize}px" +
            $"{WebConsoleDriver!.TerminalSettings.FontType}");
        // TODO: example text, actually implement
        await this._canvas2DContext.StrokeTextAsync(text: output,
            x: x,
            y: y);
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
                identifier: "window.beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.InvariantCulture),
                volume.Value.ToString(provider: CultureInfo.InvariantCulture),
                type);
        if (duration is not null && frequency is not null && volume is not null && type is null)
            await JsInterop.InvokeAsync<Task>(
                identifier: "window.beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.CurrentCulture),
                volume.Value.ToString(provider: CultureInfo.InvariantCulture));
        if (duration is not null && frequency is not null && volume is null && type is null)
            await JsInterop.InvokeAsync<Task>(
                identifier: "window.beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.InvariantCulture));
        if (duration is not null && frequency is null && volume is null && type is null)
            await JsInterop.InvokeAsync<Task>(
                identifier: "window.beep",
                duration.Value.ToString(provider: CultureInfo.CurrentCulture));
        if (duration is null && frequency is null && volume is null && type is null)
            await JsInterop.InvokeVoidAsync(
                identifier: "window.beep");
        // ReSharper restore HeapView.ObjectAllocation
    }

    private void OnReadConsoleInput(InputResult inputResult)
    {
        ReadConsoleInput?.Invoke(inputResult);
        RunIterationNeeded?.Invoke();
    }

    [JSInvokable]
    public async Task OnCanvasClick(MouseEventArgs obj)
    {
        // of relevance: ActiveConsole
        var inputResult = new InputResult()
        {
            EventType = Models.Enums.EventType.Mouse,
            MouseEvent = new WebMouseEvent()
            {
                ButtonState = Models.Enums.MouseButtonState.Button1Clicked
            }
        };
        OnReadConsoleInput(inputResult);
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
        if (_canvas2DContext == null) return;
        this.screenWidth = screenWidth;
        this.screenHeight = screenHeight;
        var inputResult = new InputResult()
        {
            EventType = Models.Enums.EventType.Resize,
            ResizeEvent = new ResizeEvent()
            {
                Size = new System.Drawing.Size(screenWidth, screenHeight)
            }
        };
        OnReadConsoleInput(inputResult);
    }

    [JSInvokable]
    public async ValueTask OnFocus()
    {
        if (_canvas2DContext == null) return;
    }

    [JSInvokable]
    public async ValueTask OnBeforeUnload()
    {
        if (_canvas2DContext == null) return;
    }
}