using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Terminal.Gui;

namespace HACC.Components;

public class WebClipboard : ComponentBase, IClipboard
{
    private static WebClipboard? _instance;

    public WebClipboard()
    {
        if (_instance != null)
            throw new InvalidOperationException(message: "Only one instance of WebClipboard is allowed.");
        _instance = this;
    }

    public static WebClipboard Instance =>
        _instance ?? throw new InvalidOperationException(message: "WebClipboard is not initialized.");

    [Parameter] public string Text { get; set; } = string.Empty;

    [Inject] private IJSRuntime JsRuntime { get; set; } = null!;

    public string? GetClipboardData()
    {
        // ReSharper disable once HeapView.DelegateAllocation
        var task = Task.Run(function: async () =>
        {
            var clipboardData = await this.JsRuntime.InvokeAsync<string>(identifier: "clipboardFunctions.getText");
            if (clipboardData is { } text) return text;
            return null;
        });
        task.Wait();
        this.Text = task.Result ?? string.Empty;
        return task.Result;
    }

    public bool TryGetClipboardData(out string result)
    {
        var clipboardData = this.GetClipboardData();
        if (clipboardData is { } text)
        {
            result = text;
            return true;
        }

        result = string.Empty;
        return false;
    }

    public void SetClipboardData(string text)
    {
        // ReSharper disable once HeapView.DelegateAllocation
        var task = Task.Run(function: async () =>
            await this.JsRuntime.InvokeAsync<bool>(identifier: "clipboardFunctions.setText", text));
        task.Wait();
        this.Text = text;
    }

    public bool TrySetClipboardData(string text)
    {
        // ReSharper disable once HeapView.DelegateAllocation
        var task = Task.Run(function: async () =>
            await this.JsRuntime.InvokeAsync<bool>(identifier: "clipboardFunctions.setText", text));
        task.Wait();
        this.Text = text;
        return task.Result;
    }

    public bool IsSupported { get; } = true;
}