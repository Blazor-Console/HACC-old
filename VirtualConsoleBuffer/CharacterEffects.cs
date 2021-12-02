namespace HACC.VirtualConsoleBuffer
{
    public struct CharacterEffects : IEquatable<CharacterEffects>
    {
        public bool Bold;
        public bool Italic;
        public bool Underline;
        public bool Inverse;
        public ConsoleColor Background;
        public ConsoleColor Foreground;

        public bool Equals(CharacterEffects other)
        {
            return
                this.Bold == other.Bold &&
                this.Italic == other.Italic &&
                this.Underline == other.Underline &&
                this.Inverse == other.Inverse &&
                this.Background == other.Background &&
                this.Foreground == other.Foreground;
        }
    }
}
