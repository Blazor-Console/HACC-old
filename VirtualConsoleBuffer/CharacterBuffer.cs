using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using System.Drawing;
using System.Globalization;

namespace HACC.VirtualConsoleBuffer
{
    public class CharacterBuffer
    {
        /// <summary>
        /// The number of columns in the buffer
        /// </summary>
        private readonly int BufferColumns;
        
        /// <summary>
        /// The number of rows in the buffer
        /// </summary>
        private readonly int BufferRows;

        /// <summary>
        /// two-dimensional array of "strings" to support multi-byte, containing the actual character buffer
        /// </summary>
        private readonly string[,] InternalBuffer;

        /// <summary>
        /// two-dimensional array of dirty state for whether the character itself has changed
        /// </summary>
        private readonly bool[,] CharacterChanged;

        /// <summary>
        /// two-dimensional array of visual state for each character
        /// </summary>
        private readonly CharacterEffects[,] CharacterEffects;
        
        /// <summary>
        /// two-dimensional array of dirty state for whether the character's appearance has changed
        /// </summary>
        private readonly bool[,] CharacterEffectsChanged;
        
        /// <summary>
        /// whether any of the appearance state is dirty
        /// </summary>
        public bool CharacterEffectsDirty { get; private set;}
        
        /// <summary>
        /// whether any of the character state is dirty
        /// </summary>
        public bool CharacterBufferDirty { get; private set; }
        
        /// <summary>
        /// Force next partial redraw to be a full redraw
        /// </summary>
        private bool ForceFullRender;
        
        private Point CursorPosition;

        /// <summary>
        /// Logging provider
        /// </summary>
        private readonly ILogger Logger;

        public CharacterBuffer(ILogger logger, int columns, int rows)
        {
            this.Logger = logger;
            this.BufferColumns = columns;
            this.BufferRows = rows;
            this.InternalBuffer = new string[columns, rows];
            this.CharacterChanged = new bool[columns, rows];
            this.CharacterEffects = new CharacterEffects[columns, rows];
            this.CharacterEffectsChanged = new bool[columns, rows];
            this.ForceFullRender = true;
            this.CursorPosition = new Point(0, 0);
        }


        public string[,] Buffer
        {
            get
            {
                string[,] newBuffer = new string[BufferColumns, BufferRows];
                for (int x = 0; x < BufferColumns; x++)
                {
                    for (int y = 0; y < BufferRows; y++)
                    {
                        newBuffer[x, y] = InternalBuffer[x, y];
                    }
                }
                return newBuffer;
            }
        }

        public void SetCharacterEffects(int x, int y, CharacterEffects effects)
        {
            var changed = !this.CharacterEffects[x, y].Equals(effects);
            this.CharacterEffects[x, y] = effects;
            this.CharacterEffectsChanged[x, y] = this.CharacterEffectsChanged[x, y] || changed;
            this.CharacterEffectsDirty = this.CharacterEffectsDirty || changed;
        }

        public void SetCharacterEffects(int xStart, int xEnd, int y, CharacterEffects effects)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                var changed = !this.CharacterEffects[x, y].Equals(effects);
                this.CharacterEffects[x, y] = effects;
                this.CharacterEffectsChanged[x, y] = this.CharacterEffectsChanged[x, y] || changed;
                this.CharacterEffectsDirty = this.CharacterEffectsDirty || changed;
            }
        }

        public string CharacterAt(int x, int y)
        {
            return this.InternalBuffer[x, y];
        }

        public string SetCharacter(int x, int y, string value, CharacterEffects? characterEffects = null)
        {
            if (x > this.BufferColumns || y > this.BufferRows)
            {
                throw new ArgumentOutOfRangeException("x and y must be less than the buffer size");
            }

            var length = GetLineElementCount(line: value, sourceStringInfo: out StringInfo stringInfo);
            if (!string.IsNullOrEmpty(value))
            {
                value = stringInfo.SubstringByTextElements(
                    startingTextElement: 0,
                    lengthInTextElements: 1);
            }
            else if (stringInfo.LengthInTextElements == 0)
            {
                value = "";
            }

            var oldValue = this.InternalBuffer[x, y];
            var changed = !oldValue.Equals(value);
            this.InternalBuffer[x, y] = value;
            this.CharacterChanged[x, y] = this.CharacterChanged[x, y] || changed;
            this.CharacterBufferDirty = this.CharacterBufferDirty || changed;
            if (characterEffects.HasValue)
            {
                var effectsChanged = this.CharacterEffects[x, y].Equals(characterEffects.Value);
                this.CharacterEffects[x, y] = characterEffects.Value;
                this.CharacterEffectsChanged[x, y] = this.CharacterEffectsChanged[x, y] || effectsChanged;
                this.CharacterEffectsDirty = this.CharacterEffectsDirty || effectsChanged;
            }
            return oldValue;
        }

        /// <summary>
        /// Gets the characters beginning at the specified coordinates, and extending up to the specified number of columns.
        /// Setting length -1 will return all characters from the start column to the end of the buffer.
        /// </summary>
        public string GetLine(int x, int y, int length = -1)
        {
            int maxLength = this.BufferColumns - x;
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

        /// <summary>
        /// Sets the characters beginning at the specified coordinates, and extending up to the specified number of columns.
        /// Also applies character effects if specified.
        /// </summary>
        public (string oldLine, int lengthWritten) SetLine(int x, int y, string line, int length = -1, CharacterEffects? characterEffects = null)
        {
            if (x > this.BufferColumns || y > this.BufferRows)
            {
                throw new ArgumentOutOfRangeException("x and y must be less than the buffer size");
            }

            int sourceLength = GetLineElementCount(line: line, sourceStringInfo: out StringInfo sourceStringInfo);
            int maxLength = this.BufferColumns - x;
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
                var oldCharacter = string.IsNullOrEmpty(oldLine) ? "" : oldStringInfo.SubstringByTextElements(
                    startingTextElement: i,
                    lengthInTextElements: 1);
                var newCharacter = sourceStringInfo.SubstringByTextElements(
                    startingTextElement: i,
                    lengthInTextElements: 1);
                var changed = !oldCharacter.Equals(newCharacter);

                this.InternalBuffer[x + i, y] = newCharacter;
                this.CharacterChanged[x + i, y] = changed || this.CharacterChanged[x + i, y]; // TODO: Why is this failing to update x: 1, y: 1??
                this.CharacterBufferDirty = this.CharacterBufferDirty || changed;

                if (characterEffects.HasValue)
                {
                    var effectsChanged = this.CharacterEffects[x + i, y].Equals(characterEffects.Value);
                    this.CharacterEffects[x + i, y] = characterEffects.Value;
                    this.CharacterEffectsChanged[x + i, y] = this.CharacterEffectsChanged[x + i, y] || effectsChanged;
                    this.CharacterEffectsDirty = this.CharacterEffectsDirty || effectsChanged;
                }
            }

            return (oldLine: oldLine, lengthWritten: length);
        }

        /// <summary>
        /// Writes a character to the buffer at the specified coordinates and advances the cursor.
        /// </summary>
        public void WriteChar(string character, CharacterEffects? characterEffects = null)
        {
            
            this.SetCharacter(
                x: this.CursorPosition.X,
                y: this.CursorPosition.Y,
                value: character,
                characterEffects: characterEffects);
            

            this.CursorPosition.X++;

            if (this.CursorPosition.X >= this.BufferColumns)
            {
                this.CursorPosition.X = 0;
                this.CursorPosition.Y++;
            }

            if (this.CursorPosition.Y >= this.BufferRows)
            {
                this.CursorPosition.Y = 0;
            }
        }

        /// <summary>
        /// Returns the number of characters in the specified string, taking into account multi-byte characters.
        /// </summary>
        private int GetLineElementCount(string line, out StringInfo sourceStringInfo)
        {
            sourceStringInfo = new StringInfo(line);

            return sourceStringInfo.LengthInTextElements;
        }

        /// <summary>
        /// Writes a string to the buffer at the specified coordinates and advances the cursor.
        /// </summary>
        public void WriteLine(string line, CharacterEffects? characterEffects = null)
        {
            var newLength = GetLineElementCount(line: line, sourceStringInfo: out StringInfo sourceStringInfo);
            (string oldLine, int lengthWritten) = this.SetLine(
                x: this.CursorPosition.X,
                y: this.CursorPosition.Y,
                line: line,
                characterEffects: characterEffects);

                this.CursorPosition.X += lengthWritten;
                this.CursorPosition.Y++;
        }

        public CharacterEffects CharacterEffectsAt(int x, int y) => this.CharacterEffects[x, y];

        /// <summary>
        /// whether the character at the given position has changed
        /// </summary>
        public bool CharacterDirty(int x, int y) => this.CharacterChanged[x, y];

        /// <summary>
        /// whether the character effects at the given position have changed
        /// </summary>
        public bool EffectsDirty(int x, int y) => this.CharacterEffectsChanged[x, y];

        /// <summary>
        /// Returns the coordinates of all section marked dirty
        /// </summary>
        public IEnumerable<(int y, int xStart, int xEnd)> DirtyRanges(bool includeEffectsChanges = true)
        {
                var list = new List<(int y, int xStart, int xEnd)>();
                for (int y = 0; y < BufferRows; y++)
                {
                    int changeStart = -1;
                    CharacterEffects? lastEffects = null;
                    for (int x = 0; x < BufferColumns; x++)
                    {
                        var effectsChanged = includeEffectsChanges && (!lastEffects.HasValue || lastEffects.Equals(this.CharacterEffects[x, y]));
                        var changed = this.CharacterChanged[x, y] && !effectsChanged;

                        if (changed && (changeStart < -1))
                        {
                            changeStart = x;
                        }

                        if ((changeStart >= 0) && (!changed || effectsChanged))
                        {
                            list.Add((y: y, xStart: changeStart, xEnd: x));
                            changeStart = -1;
                        }

                        lastEffects = this.CharacterEffects[x, y];
                    }

                    if (changeStart >= 0)
                    {
                        list.Add((y: y, xStart: changeStart, xEnd: BufferColumns - 1));
                    }
                }

                return list;
        }

        /// <summary>
        /// Returns a collection of ranges marked dirty and their corresponding text and character effects
        /// </summary>
        public IEnumerable<(int xStart, int xEnd, int y, string value, CharacterEffects effects)> DirtyRangeValues(bool includeEffectsChanges = true)
        {
            var list = new List<(int xStart, int xEnd, int y, string value, CharacterEffects effects)>();
            var ranges = DirtyRanges(includeEffectsChanges: includeEffectsChanges);
            foreach (var range in ranges)
            {
                list.Add((
                    xStart: range.xStart,
                    xEnd: range.xEnd,
                    y: range.y,
                    value: GetLine(
                        x: range.xStart,
                        y: range.y,
                        length: range.xEnd - range.xStart + 1),
                    effects: this.CharacterEffects[range.xStart, range.y]));
            }
            return list;
        }

        /// <summary>
        /// Returns a new buffer with the given dimension changes applied.
        /// Lines will be truncated if the new area is smaller.
        /// </summary>
        public CharacterBuffer Resize(int newColumns, int newRows)
        {
            var newBuffer = new CharacterBuffer(
                logger: this.Logger,
                columns: newColumns,
                rows: newRows);

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
            this.Logger.LogInformation("Partial rendering requested");

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
