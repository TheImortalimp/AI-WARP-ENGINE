'use strict';

async function runFreezeFrameChecks() {
    const engine = window.aiWarpEngine;
    if (!engine) throw new Error('Engine not initialized.');
    cancelAnimationFrame(engine.animFrameId);
    const canvas = engine.canvas;
    const context = canvas.getContext('2d');
    context.clearRect(0, 0, canvas.width, canvas.height);
    context.fillStyle = '#609080';
    context.fillRect(0, 0, canvas.width, canvas.height);
    canvas.style.filter = 'none';
    document.documentElement.style.filter = 'none';
    const check = (condition, message) => { if (!condition) throw new Error(message); };
    async function pixels(url) {
        const image = new Image();
        image.src = url;
        await image.decode();
        const result = document.createElement('canvas');
        result.width = image.width;
        result.height = image.height;
        const ctx = result.getContext('2d');
        ctx.drawImage(image, 0, 0);
        return { width: image.width, height: image.height,
            at: (x, y) => [...ctx.getImageData(x, y, 1, 1).data] };
    }
    const capture = new FreezeFrameCapture(window.chrome.webview);
    const native = await pixels(await capture.capture(canvas, 'video'));
    check(native.width > 0 && native.height > 0, 'Empty native capture.');
    check(!document.documentElement.classList.contains('freeze-frame-capture'), 'HUD mask not restored.');
    const center = native.at(Math.floor(native.width / 2), Math.floor(native.height / 2));
    const edge = native.at(1, Math.floor(native.height / 2));
    check(center[1] > 90, 'Canvas/media base missing from native capture.');
    check(edge[1] < center[1] * 0.7, 'Fullscreen vignette missing from native capture.');
    const top = native.at(Math.floor(native.width / 2), 15);
    check(top[1] > 5 && top[1] < center[1], 'HUD was captured instead of masked FX.');
    const browser = await pixels(await new FreezeFrameCapture(null).capture(canvas, 'video'));
    const browserCenter = browser.at(Math.floor(browser.width / 2), Math.floor(browser.height / 2));
    const browserEdge = browser.at(1, Math.floor(browser.height / 2));
    check(browserEdge[1] < browserCenter[1] * 0.7, 'Browser fullscreen vignette missing.');
    document.documentElement.style.filter = 'invert(1)';
    const inverted = await pixels(await capture.capture(canvas, 'video'));
    const invertedCenter = inverted.at(Math.floor(inverted.width / 2), Math.floor(inverted.height / 2));
    check(Math.abs(invertedCenter[1] - (255 - center[1])) <= 3, 'Page filter missing or applied twice.');
    document.documentElement.style.filter = 'none';
    // Pixel-producing layers behind the final FX canvas must survive compositor capture.
    context.clearRect(0, 0, canvas.width, canvas.height);
    const shell = document.getElementById('ytWarpShell');
    shell.style.cssText = 'display:block;position:absolute;inset:0;width:100%;height:100%;background:rgb(40,170,80)';
    const media = await pixels(await capture.capture(canvas, 'youtube'));
    const mediaCenter = media.at(Math.floor(media.width / 2), Math.floor(media.height / 2));
    check(mediaCenter[1] > 140 && mediaCenter[0] < 60, 'Underlying LENS/media layer missing.');
    let rejected = false;
    try { await new FreezeFrameCapture(null).capture(canvas, 'youtube'); }
    catch (error) { rejected = /Share the YouTube/.test(error.message); }
    check(rejected, 'Browser cross-origin capture did not fail explicitly.');
    return { result: 'passed', nativeSize: [native.width, native.height],
        nativeCenter: center, nativeEdge: edge, browserCenter, browserEdge,
        invertedCenter, mediaCenter };
}

window.runFreezeFrameChecks = runFreezeFrameChecks;
