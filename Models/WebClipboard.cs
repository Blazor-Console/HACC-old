using Terminal.Gui;

namespace HACC.Models;

public class WebClipboard : IClipboard
{
    public string GetClipboardData()
    {
        throw new NotImplementedException();
    }

    public bool TryGetClipboardData(out string result)
    {
        throw new NotImplementedException();
    }

    public void SetClipboardData(string text)
    {
        throw new NotImplementedException();
    }

    public bool TrySetClipboardData(string text)
    {
        throw new NotImplementedException();
    }

    public bool IsSupported { get; } = true;
}