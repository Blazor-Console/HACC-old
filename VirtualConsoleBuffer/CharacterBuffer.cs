using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using System.Globalization;

namespace HACC.VirtualConsoleBuffer
{
    public class CharacterBuffer
    {
        private readonly int CharacterWidth;
        private readonly int CharacterHeight;

        /// <summary>
        /// two-dimensional array of "strings" to support multi-byte
        /// </summary>
        private readonly string[,] InternalBuffer;
        private readonly bool[,] CharacterChanged;
        private bool CharacterBufferChanged;
        private bool ForceFullRender;
        private readonly ILogger Logger;

        public CharacterBuffer(ILogger logger, int characterWidth, int characterHeight)
        {
            this.Logger = logger;
            this.CharacterWidth = characterWidth;
            this.CharacterHeight = characterHeight;
            this.InternalBuffer = new string[characterWidth, characterHeight];
            this.CharacterChanged = new bool[characterWidth, characterHeight];
            this.ForceFullRender = true;
        }

        public bool Dirty => this.CharacterBufferChanged;

        public string[,] Buffer
        {
            get
            {
                string[,] newBuffer = new string[CharacterWidth, CharacterHeight];
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

        public string CharacterAt(int x, int y)
        {
            return this.InternalBuffer[x, y];
        }

        public string SetCharacter(int x, int y, string value)
        {
            StringInfo stringInfo = new StringInfo(value);
            if (stringInfo.LengthInTextElements > 1)
            {
                value = stringInfo.SubstringByTextElements(
                    startingTextElement: 0,
                    lengthInTextElements: 1);
            }
            else if (stringInfo.LengthInTextElements == 0)
            {
                value = "\0";
            }

            var oldValue = this.InternalBuffer[x, y];
            var changed = !oldValue.Equals(value);
            this.InternalBuffer[x, y] = value;
            this.CharacterChanged[x, y] = this.CharacterChanged[x, y] || changed;
            this.CharacterBufferChanged = this.CharacterBufferChanged || changed;
            return oldValue;
        }

        public string GetLine(int x, int y, int length = -1)
        {
            int maxLength = this.CharacterWidth - x;
            if ((length < 0) || (length > maxLength))
            {
                length = maxLength;
            }
            string[] substrings = new string[length];
            for (int i = 0; i < length; i++)
            {
                substrings[i] = this.InternalBuffer[x + i, y];
            }
            return string.Concat(values: substrings);
        }

        public string SetLine(int x, int y, string line, int length = -1)
        {
            StringInfo sourceStringInfo = new StringInfo(line);

            int sourceLength = line.Count();
            int maxLength = this.CharacterWidth - x;
            if ((length < 0) || (length > maxLength))
            {
                length = maxLength;
            }
            if (length > sourceLength)
            {
                length = sourceLength;
            }

            var oldLine = GetLine(
                x: x,
                y: y,
                length: length);

            StringInfo oldStringInfo = new StringInfo(oldLine);
            for (int i = 0; i < length; i++)
            {
                var oldCharacter = oldStringInfo.SubstringByTextElements(
                    startingTextElement: i,
                    lengthInTextElements: 1);
                var newCharacter = sourceStringInfo.SubstringByTextElements(
                    startingTextElement: i,
                    lengthInTextElements: 1);
                var changed = !oldCharacter.Equals(newCharacter);

                this.InternalBuffer[x + i, y] = newCharacter;
                this.CharacterChanged[x + i, y] = this.CharacterChanged[x + i, y] || changed;
                this.CharacterBufferChanged = this.CharacterBufferChanged || changed;
            }

            return oldLine;
        }

        public bool CharacterDirty(int x, int y) => this.CharacterChanged[x, y];

        public IEnumerable<(int y, int xStart, int xEnd)> DirtyRanges
        {
            get
            {
                var list = new List<(int y, int xStart, int xEnd)>();
                for (int y = 0; y < CharacterHeight; y++)
                {
                    int changeStart = -1;
                    for (int x = 0; x < CharacterWidth; x++)
                    {
                        if (this.CharacterChanged[x, y] && (changeStart < -1))
                        {
                            changeStart = x;
                        }
                        if ((changeStart >= 0) && !this.CharacterChanged[x, y])
                        {
                            list.Add((y: y, xStart: changeStart, xEnd: x));
                            changeStart = -1;
                        }
                    }

                    if (changeStart >= 0)
                    {
                        list.Add((y: y, xStart: changeStart, xEnd: CharacterWidth - 1));
                    }
                }

                return list;
            }
        }

        public IEnumerable<(int xStart, int xEnd, int y, string value)> DirtyRangeStrings
        {
            get
            {
                var list = new List<(int xStart, int xEnd, int y, string value)>();
                var ranges = DirtyRanges;
                foreach (var range in ranges)
                {
                    list.Add((
                        xStart: range.xStart,
                        xEnd: range.xEnd,
                        y: range.y,
                        value: GetLine(
                            x: range.xStart,
                            y: range.y,
                            length: range.xEnd - range.xStart + 1)));
                }
                return list;
            }
        }

        public CharacterBuffer Resize(int newCharacterWidth, int newCharacterHeight)
        {
            var newBuffer = new CharacterBuffer(
                logger: this.Logger,
                characterWidth: newCharacterWidth,
                characterHeight: newCharacterHeight);

            // TODO: copy
            throw new NotImplementedException();

            return newBuffer;
        }

        /// <summary>
        /// Redraws the entire canvas
        /// </summary>
        /// <param name="context"></param>
        /// <param name="canvas"></param>
        public void RenderFull(Canvas2DContext context, BECanvasComponent canvas)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Renders only the changes since last render
        /// </summary>
        /// <param name="context"></param>
        /// <param name="canvas"></param>
        public void RenderUpdates(Canvas2DContext context, BECanvasComponent canvas)
        {
            if (this.ForceFullRender)
            {
                this.Logger.LogInformation("Full render forced");
                RenderFull(
                    context: context,
                    canvas: canvas);
                this.ForceFullRender = false;
                return;
            }

            throw new NotImplementedException();
        }
    }
}
