using HACC.Configuration;

namespace HACC.Models;

public struct CharacterEffects : IEquatable<CharacterEffects>
{
    public bool Bold;
    public bool Italic;
    public bool Underline;
    public bool Inverse;
    public bool Blink;
    public ConsoleColor Background;
    public ConsoleColor Foreground;

    public bool Equals(CharacterEffects other)
    {
        return
            Bold == other.Bold &&
            Italic == other.Italic &&
            Underline == other.Underline &&
            Inverse == other.Inverse &&
            Blink == other.Blink &&
            Background == other.Background &&
            Foreground == other.Foreground;
    }

    public static bool operator ==(CharacterEffects a, CharacterEffects? b)
    {
        return b is null ? false : a.Equals(obj: b);
    }

    public static bool operator !=(CharacterEffects a, CharacterEffects? b)
    {
        return b is null ? false : !a.Equals(obj: b);
    }

    public CharacterEffects(bool bold = false, bool italic = false, bool underline = false, bool inverse = false,
        bool blink = false, ConsoleColor background = Defaults.BackgroundColor,
        ConsoleColor foreground = Defaults.ForegroundColor)
    {
        Bold = bold;
        Italic = italic;
        Underline = underline;
        Inverse = inverse;
        Blink = blink;
        Background = background;
        Foreground = foreground;
    }

    public CharacterEffects()
    {
        Bold = false;
        Italic = false;
        Underline = false;
        Inverse = false;
        Blink = false;
        Background = Defaults.BackgroundColor;
        Foreground = Defaults.ForegroundColor;
    }

    public CharacterEffects Copy()
    {
        return new CharacterEffects(
            bold: Bold,
            italic: Italic,
            underline: Underline,
            inverse: Inverse,
            blink: Blink,
            background: Background,
            foreground: Foreground);
    }
}