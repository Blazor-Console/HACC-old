using System.Globalization;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using HACC.Enumerations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace HACC.Components;

public partial class WebConsole : ComponentBase
{
    private readonly ILogger _logger;

    //private readonly WebConsoleDriver _webConsoleDriver;

    private Canvas2DContext? _canvas2DContext;

    private ElementReference _divCanvas;
    private BECanvasComponent _beCanvas;

    /// <summary>
    ///     Initializes a web console.
    /// </summary>
    /// <param name="logger">dependency injected logger</param>
    /// <exception cref="ArgumentNullException"></exception>
    //public WebConsole(ILogger logger, WebConsoleDriver webConsoleDriver)
    //{
    //    this._logger = logger;
    //    this._webConsoleDriver = webConsoleDriver ?? throw new ArgumentNullException(paramName: nameof(webConsoleDriver),
    //        message: WebStrings.ConsoleDriverRequired);
    //}
    //public WebConsole(ILogger logger)
    //{
    //    this._logger = logger;
    //}

    [Inject] private IJSRuntime JsInterop { get; set; } = default!;

    protected new async Task OnAfterRenderAsync(bool firstRender)
    {
        this._logger.LogDebug(message: "OnAfterRenderAsync");
        await base.OnAfterRenderAsync(firstRender: firstRender);
        //this._webConsoleDriver.UpdateScreen(firstRender: firstRender);
        this._logger.LogDebug(message: "OnAfterRenderAsync: end");
    }

    public async Task<object> DrawBufferToPng()
    {
        return await this.JsInterop.InvokeAsync<object>(identifier: "window.canvasToPng");
    }

    private async Task RedrawCanvas()
    {
        this._logger.LogDebug(message: "InitializeNewCanvasFrame");

        // TODO: actually clear the canvas
        //await this._canvas2DContextStdOut.SetFillStyleAsync(value: "blue");
        //await this._canvas2DContextStdOut.ClearRectAsync(
        //    x: 0,
        //    y: 0,
        //    width: this._webConsoleDriver.WindowWidthPixels,
        //    height: this._webConsoleDriver.WindowHeightPixels);
        //await this._canvas2DContextStdOut.FillRectAsync(
        //    x: 0,
        //    y: 0,
        //    width: this._webConsoleDriver.WindowWidthPixels,
        //    height: this._webConsoleDriver.WindowHeightPixels);


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
        this._logger.LogDebug(message: "InitializeNewCanvasFrame: end");
    }

    public async Task DrawUpdatesToCanvas(int[,,] buffer, bool? firstRender = null)
    {
        this._logger.LogDebug(message: "DrawBufferToFrame");
        if (firstRender.HasValue && firstRender.Value || this._canvas2DContext is null)
            await this.RedrawCanvas();
        await this._canvas2DContext!.SetFontAsync(value: "8px serif");
        // TODO: example text, actually implement
        await this._canvas2DContext.StrokeTextAsync(text: "drawing changes from dirty lines....",
            x: 10,
            y: 100);
        this._logger.LogDebug(message: "DrawBufferToFrame: end");
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
            await this.JsInterop.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.InvariantCulture),
                volume.Value.ToString(provider: CultureInfo.InvariantCulture),
                type);
        if (duration is not null && frequency is not null && volume is not null && type is null)
            await this.JsInterop.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.CurrentCulture),
                volume.Value.ToString(provider: CultureInfo.InvariantCulture));
        if (duration is not null && frequency is not null && volume is null && type is null)
            await this.JsInterop.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.InvariantCulture),
                frequency.Value.ToString(provider: CultureInfo.InvariantCulture));
        if (duration is not null && frequency is null && volume is null && type is null)
            await this.JsInterop.InvokeAsync<Task>(
                identifier: "beep",
                duration.Value.ToString(provider: CultureInfo.CurrentCulture));
        if (duration is null && frequency is null && volume is null && type is null)
            await this.JsInterop.InvokeVoidAsync(
                identifier: "beep");
        // ReSharper restore HeapView.ObjectAllocation
    }

    private async Task OnCanvasClick(MouseEventArgs obj)
    {
        // of relevance: ActiveConsole
        throw new NotImplementedException();
    }

    private async Task OnCanvasKeyDown(KeyboardEventArgs obj)
    {
        // of relevance: ActiveConsole
        throw new NotImplementedException();
    }

    private async Task OnCanvasKeyUp(KeyboardEventArgs obj)
    {
        // of relevance: ActiveConsole
        throw new NotImplementedException();
    }

    private async Task OnCanvasKeyPress(KeyboardEventArgs arg)
    {
        // of relevance: ActiveConsole
        throw new NotImplementedException();
    }
}