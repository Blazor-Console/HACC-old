using HACC.Models.Enums;

namespace HACC.Models.Structs;

public struct InputResult
{
	public EventType EventType;
	public ConsoleKeyInfo ConsoleKeyInfo;
	public MouseEvent MouseEvent;
	public ResizeEvent ResizeEvent;
}
