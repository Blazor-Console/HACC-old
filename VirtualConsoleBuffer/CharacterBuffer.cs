using System.Drawing;

namespace HACC.VirtualConsoleBuffer
{
    public class CharacterBuffer<T>
    {
        private readonly int CharacterWidth;
        private readonly int CharacterHeight;
        private readonly T[,] InternalBuffer;
        private readonly bool[,] CharacterChanged;
        private bool CharacterBufferChanged;

        public CharacterBuffer(int characterWidth, int characterHeight)
        {
            this.CharacterWidth = characterWidth;
            this.CharacterHeight = characterHeight;
            this.InternalBuffer = new T[characterWidth, characterHeight];
            this.CharacterChanged = new bool[characterWidth, characterHeight];
        }

        public bool Dirty => this.CharacterBufferChanged;

        public T[,] Buffer
        {
            get
            {
                T[,] newBuffer = new T[CharacterWidth, CharacterHeight];
                for (int x = 0; x < CharacterWidth; x++)
                {
                    for (int y = 0; y < CharacterHeight; y++)
                    {
                        newBuffer[x, y] = InternalBuffer[x, y];
                    }
                }
                return newBuffer;
            }
        }

        public T CharacterAt(int x, int y)
        {
            return this.InternalBuffer[x, y];
        }

        public T SetCharacter(int x, int y, T value)
        {
            var oldValue = this.InternalBuffer[x, y];
            var changed = !oldValue.Equals(value);
            this.InternalBuffer[x, y] = value;
            this.CharacterChanged[x, y] = this.CharacterChanged[x, y] || changed;
            this.CharacterBufferChanged = this.CharacterBufferChanged || changed;
            return oldValue;
        }

        public bool CharacterDirty(int x, int y) => this.CharacterChanged[x, y];

        public CharacterBuffer<T> Resize(int newCharacterWidth, int newCharacterHeight)
        {
            var newBuffer = new CharacterBuffer<T>(
                characterWidth: newCharacterWidth,
                characterHeight: newCharacterHeight);

            // TODO: copy
            throw new NotImplementedException();

            return newBuffer;
        }
    }
}
