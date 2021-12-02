namespace HACC.Components
{
    using Blazor.Extensions;
    using Blazor.Extensions.Canvas.Canvas2D;
    using HACC.VirtualConsoleBuffer;

    public partial class Console
    {
        private Canvas2DContext _context;

        protected BECanvasComponent _canvasReference;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            this._context = await this._canvasReference.CreateCanvas2DAsync();
            await this._context.SetFillStyleAsync("green");

            await this._context.FillRectAsync(10, 100, 100, 100);

            await this._context.SetFontAsync("48px serif");
            await this._context.StrokeTextAsync("Hello Blazor!!!", 10, 100);
        }

        public void RenderFullCharacterBuffer(CharacterBuffer characterBuffer)
        {
            characterBuffer.RenderFull(
                context: _context,
                canvas: _canvasReference);
        }

        public void RenderUpdatesFromCharacterBuffer(CharacterBuffer characterBuffer)
        {
            characterBuffer.RenderUpdates(
                context: _context,
                canvas: _canvasReference);
        }
    }
}
