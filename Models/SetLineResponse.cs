namespace HACC.Models;

public struct SetLineResponse
{
    public readonly string TextReplaced;
    public readonly int LengthWritten;
    public readonly string TextOverflow;

    public SetLineResponse(string textReplaced, int lengthWritten, string textOverflow)
    {
        TextReplaced = textReplaced;
        LengthWritten = lengthWritten;
        TextOverflow = textOverflow;
    }
}