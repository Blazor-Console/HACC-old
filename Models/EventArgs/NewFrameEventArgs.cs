using HACC.Models.Drivers;

namespace HACC.Models.EventArgs;

public record NewFrameEventArgs : VirtualConsoleEventArgs
{
    public NewFrameEventArgs(CanvasConsole sender) : base(sender: sender)
    {
        throw new NotImplementedException();
    }
}