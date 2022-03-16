using System.Globalization;
using System.Runtime.Versioning;
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

[SupportedOSPlatform(platformName: "browser")]
public partial class WebConsole : ComponentBase
{
    private static WebConsole? _singleton = null;
    private readonly ILogger _logger;
    private readonly WebConsoleDriver _webConsoleDriver;

    private Canvas2DContext? _canvas2DContextStdErr = null;

    /// <summary>
    ///     not created until OnAfterRender
    /// </summary>
    private Canvas2DContext? _canvas2DContextStdOut = null;

    /// <summary>
    /// </summary>
    /// <param name="logger">dependency injected logger</param>
    /// <param name="webClipboard">dependency injected clipboard</param>
    /// <exception cref="InvalidOperationException"></exception>
    public WebConsole(ILogger logger, WebClipboard webClipboard)
    {
        if (_singleton is not null)
            throw new InvalidOperationException(message: "ConsoleDriver can only be instantiated once.");
        _singleton = this;
        this._logger = logger;
        this._webConsoleDriver = new WebConsoleDriver(
            logger: this._logger ?? throw new InvalidOperationException(),
            webClipboard: webClipboard,
            console: this,
            terminalSettings: new TerminalSettings());
    }

    [Parameter] public ConsoleType ActiveConsole { get; set; } = ConsoleType.StandardOutput;

    private BECanvasComponent CanvasReferenceStdOut { get; set; } = null!;
    private BECanvasComponent CanvasReferenceStdErr { get; set; } = null!;

    [Inject] private IJSRuntime JsInterop { get; set; } = null!;

    public static WebConsole Instance =>
        _singleton ?? throw new ArgumentException(message: "ConsoleDriver not instantiated");

    protected new async Task OnAfterRenderAsync(bool firstRender)
    {
        this._logger.LogDebug(message: "OnAfterRenderAsync");
        await base.OnAfterRenderAsync(firstRender: firstRender);
        this._webConsoleDriver.UpdateScreen(firstRender: firstRender);
        this._logger.LogDebug(message: "OnAfterRenderAsync: end");
    }

    private async Task InitializeNewCanvasFrame()
    {
        this._logger.LogDebug(message: "InitializeNewCanvasFrame");
        this._canvas2DContextStdOut = await this.CanvasReferenceStdOut.CreateCanvas2DAsync();
        this._canvas2DContextStdErr = await this.CanvasReferenceStdErr.CreateCanvas2DAsync();

        await this._canvas2DContextStdOut.SetFillStyleAsync(value: "blue");
        await this._canvas2DContextStdOut.ClearRectAsync(
            x: 0,
            y: 0,
            width: this._webConsoleDriver.WindowWidthPixels,
            height: this._webConsoleDriver.WindowHeightPixels);
        await this._canvas2DContextStdOut.FillRectAsync(
            x: 0,
            y: 0,
            width: this._webConsoleDriver.WindowWidthPixels,
            height: this._webConsoleDriver.WindowHeightPixels);


        await this._canvas2DContextStdErr.SetFillStyleAsync(value: "blue");
        await this._canvas2DContextStdErr.ClearRectAsync(
            x: 0,
            y: 0,
            width: this._webConsoleDriver.WindowWidthPixels,
            height: this._webConsoleDriver.WindowHeightPixels);
        await this._canvas2DContextStdErr.FillRectAsync(
            x: 0,
            y: 0,
            width: this._webConsoleDriver.WindowWidthPixels,
            height: this._webConsoleDriver.WindowHeightPixels);
        this._logger.LogDebug(message: "InitializeNewCanvasFrame: end");
    }

    public async Task DrawBufferToNewFrame(int[,,] buffer, bool? firstRender = null)
    {
        this._logger.LogDebug(message: "DrawBufferToFrame");
        if (firstRender.HasValue && firstRender.Value || this._canvas2DContextStdOut is null)
            await this.InitializeNewCanvasFrame();
        // draw changes from dirty lines
        await this._canvas2DContextStdOut!.SetFontAsync(value: "8px serif");
        await this._canvas2DContextStdOut.StrokeTextAsync(text: "blah",
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