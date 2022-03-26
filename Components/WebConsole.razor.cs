using System.Globalization;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using HACC.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace HACC.Components;

public partial class WebConsole : ComponentBase
{
    private static readonly IJSRuntime JsInterop = HaccExtensions.GetService<IJSRuntime>();
    private static readonly ILogger Logger = HaccExtensions.CreateLogger<WebConsole>();
    private BECanvasComponent? _beCanvas;

    private Canvas2DContext? _canvas2DContext;

    private ElementReference _divCanvas;

    protected new async Task OnAfterRenderAsync(bool firstRender)
    {
        Logger.LogDebug(message: "OnAfterRenderAsync");
        await base.OnAfterRenderAsync(firstRender: firstRender);
        //this._webConsoleDriver.UpdateScreen(firstRender: firstRender);
        Logger.LogDebug(message: "OnAfterRenderAsync: end");
    }

    public async Task<object> DrawBufferToPng()
    {
        return await JsInterop.InvokeAsync<object>(identifier: "window.canvasToPng");
    }

    private async Task RedrawCanvas()
    {
        Logger.LogDebug(message: "InitializeNewCanvasFrame");

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
        Logger.LogDebug(message: "InitializeNewCanvasFrame: end");
    }

    public async Task DrawUpdatesToCanvas(int[,,] buffer, bool? firstRender = null)
    {
        Logger.LogDebug(message: "DrawBufferToFrame");
        if (firstRender.HasValue && firstRender.Value || this._canvas2DContext is null)
            await this.RedrawCanvas();
        await this._canvas2DContext!.SetFontAsync(value: "8px serif");
        // TODO: example text, actually implement
        await this._canvas2DContext.StrokeTextAsync(text: "drawing changes from dirty lines....",
            x: 10,
            y: 100);
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