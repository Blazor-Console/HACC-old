namespace HACC.VirtualConsoleBuffer
{
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
                this.Bold == other.Bold &&
                this.Italic == other.Italic &&
                this.Underline == other.Underline &&
                this.Inverse == other.Inverse &&
                this.Blink == other.Blink &&
                this.Background == other.Background &&
                this.Foreground == other.Foreground;
        }

        public CharacterEffects(bool bold = false, bool italic = false, bool underline = false, bool inverse = false, bool blink = false, ConsoleColor background = Defaults.BackgroundColor, ConsoleColor foreground = Defaults.ForegroundColor)
        {
            this.Bold = bold;
            this.Italic = italic;
            this.Underline = underline;
            this.Inverse = inverse;
            this.Blink = blink;
            this.Background = background;
            this.Foreground = foreground;
        }
    }
}
