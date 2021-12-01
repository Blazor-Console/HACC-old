using System.Drawing;

namespace HACC.VirtualConsoleBuffer
{
    public struct TerminalSettings
    {
        public string Title;
        public int TerminalWidth;
        public int TerminalHeight;
        public int CharacterWidth;
        public int CharacterHeight;
        public bool CursorVisible;
        public bool StatusVisible;
        public bool TitleVisible;
        public Point CursorPosition;

        /// <summary>
        /// Cursor height in percentage of character
        /// </summary>
        public int CursorHeight;

        /// <summary>
        /// Summary:
        ///     Gets or sets the height of the cursor within a character cell.
        ///
        /// Returns:
        ///     The size of the cursor expressed as a percentage of the height of a character
        ///     cell. The property value ranges from 1 to 100.
        /// </summary>
        public int CursorSize;

        public ConsoleColor TerminalBackground;
        public ConsoleColor TerminalForeground;

        public TerminalSettings()
        {
            Title = "";
            TerminalWidth = Defaults.InitialTerminalWidth;
            TerminalHeight = Defaults.InitialTerminalHeight;
            CharacterWidth = Defaults.InitialCharacterWidth;
            CharacterHeight = Defaults.InitialCharacterHeight;
            CursorVisible = Defaults.CursorVisibility;
            StatusVisible = Defaults.StatusVisibility;
            TitleVisible = Defaults.TitleVisibility;
            CursorPosition = new Point(
                x: 0,
                y: 0);
            CursorHeight = 100;
            CursorSize = 100;
            TerminalForeground = ConsoleColor.White;
            TerminalBackground = ConsoleColor.Black;
        }

    }
}
