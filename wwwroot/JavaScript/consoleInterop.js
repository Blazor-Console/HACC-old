function onResize() {
    if (!window.console.canvas)
        return;

    console.canvas.width = window.innerWidth;
    console.canvas.height = window.innerHeight;

    console.instance.invokeMethodAsync('OnResize', console.canvas.width, console.canvas.height);
}

function onFocus() {
    if (!window.console.canvas)
        return;

    console.instance.invokeMethodAsync('OnFocus');
}

window.consoleWindowResize = (instance) => {
    onResize();
};

window.consoleWindowFocus = (instance) => {
    onFocus();
}
window.initConsole = (instance) => {
    var canvasContainer = document.getElementById('_divCanvas'),
        canvases = canvasContainer.getElementsByTagName('canvas') || [];
    window.console = {
        instance: instance,
        canvas: canvases.length ? canvases[0] : null
    };

    if (window.console.canvas) {
        window.console.canvas.onmousemove = (e) => {
            console.instance.invokeMethodAsync('OnCanvasClick', e);
        };
        window.console.canvas.onmousedown = (e) => {
            console.instance.invokeMethodAsync('OnCanvasClick', e);
        };
        window.console.canvas.onmouseup = (e) => {
            console.instance.invokeMethodAsync('OnCanvasClick', e);
        };

        window.console.canvas.onkeydown = (e) => {
            console.instance.invokeMethodAsync('OnCanvasKeyDown', e);
        };
        window.console.canvas.onkeyup = (e) => {
            console.instance.invokeMethodAsync('OnCanvasKeyUp', e);
        };
        window.console.canvas.onblur = (e) => {
            window.console.canvas.focus();
        };
        window.console.canvas.tabIndex = 0;
        window.console.canvas.focus();
    }

    window.addEventListener("resize", onResize);
    window.addEventListener("focus", onFocus);
};
