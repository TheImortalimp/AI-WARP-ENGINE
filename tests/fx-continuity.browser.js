'use strict';

// Load this script in the running browser app, then await runFxContinuityChecks().
async function runFxContinuityChecks() {
    const check = (condition, message) => {
        if (!condition) throw new Error(message);
    };
    const canvas = document.createElement('canvas');
    canvas.style.cssText = 'position:fixed;left:-1000px;width:160px;height:90px';
    document.body.appendChild(canvas);
    const engine = new window.AiWarpEngine(canvas);
    cancelAnimationFrame(engine.animFrameId);
    engine.resize = () => {};
    const source = document.createElement('canvas');
    source.width = canvas.width;
    source.height = canvas.height;
    const sourceCtx = source.getContext('2d');
    for (let x = 0; x < source.width; x += 8) {
        sourceCtx.fillStyle = x % 16 ? '#304050' : '#a0b0c0';
        sourceCtx.fillRect(x, 0, 8, source.height);
    }

    window.runFxContinuityChecks = runFxContinuityChecks;
    const raw = sourceCtx.getImageData(0, 0, source.width, source.height).data;
    engine.setSourceMode('image', source);
    engine.activeEffects = [];
    engine.masterOpacity = 0.88;
    let now = 100000;
    let sampledFrames = 0;
    let bufferResets = 0;
    const originalWidth = Object.getOwnPropertyDescriptor(HTMLCanvasElement.prototype, 'width');
    Object.defineProperty(engine.workCanvas, 'width', {
        get() { return originalWidth.get.call(this); },
        set(value) { bufferResets++; originalWidth.set.call(this, value); },
        configurable: true
    });
    try {
        for (const strength of [0.02, 0.65, 1]) {
            engine.setAiWarpStrength(strength);
            for (const mode of engine.aiWarpModes.filter(mode => mode !== 'ai_zero')) {
                engine.setWarpMode(mode);
                for (let frame = 0; frame < 30; frame++) {
                    now += 1000 / 60;
                    engine.glitchSpeed = 0.4 + frame / 15;
                    engine.renderFrame(now, 1 / 60);
                    const pixels = engine.gctx.getImageData(0, 0, canvas.width, canvas.height).data;
                    let changed = 0;
                    for (let i = 0; i < pixels.length; i += 4) {
                        check(pixels[i + 3] === 255, `${mode}/${strength}: transparent pixel at frame ${frame}`);
                        if (pixels[i] !== raw[i] || pixels[i + 1] !== raw[i + 1] || pixels[i + 2] !== raw[i + 2]) changed++;
                    }
                    check(changed > 0, `${mode}/${strength}: untreated frame ${frame}`);
                    sampledFrames++;
                }
            }
        }
        check(bufferResets === 0, 'Scratch buffers were resized during steady-state rendering');

        engine.setWarpMode('ai_zero');
        engine.setGhostMode('OFF', 0);
        engine.renderFrame(now += 17, 0.017);
        let pixels = engine.gctx.getImageData(0, 0, canvas.width, canvas.height).data;
        check(pixels.every((value, i) => value === raw[i]), 'AI ZERO retained warp graphics');
        engine.setWarpMode('liquid');
        engine.renderFrame(now += 17, 0.017);
        engine.setAiWarpActive(false);
        engine.renderFrame(now += 17, 0.017);
        pixels = engine.gctx.getImageData(0, 0, canvas.width, canvas.height).data;
        check(pixels.every((value, i) => value === raw[i]), 'OFF retained warp graphics');

        engine.setAiWarpActive(true);
        engine.mainRenderLoop(now += 17);
        cancelAnimationFrame(engine.animFrameId);
        check(engine.hasCompletedFrame, `Initial completed frame failed: ${Array.from(engine.renderErrors)}`);
        const completed = engine.gctx.getImageData(0, 0, canvas.width, canvas.height).data;
        const draw = engine.warpCtx.drawImage;
        const report = engine.reportRenderError;
        const reported = [];
        engine.reportRenderError = (stage, error) => reported.push([stage, error.message]);
        engine.warpCtx.drawImage = () => { throw new Error('injected slice failure'); };
        engine.mainRenderLoop(now += 17);
        cancelAnimationFrame(engine.animFrameId);
        pixels = engine.gctx.getImageData(0, 0, canvas.width, canvas.height).data;
        const mismatch = pixels.findIndex((value, i) => value !== completed[i]);
        check(mismatch === -1, `Failed warp exposed a partial/raw frame at ${mismatch}: ${pixels[mismatch]} vs ${completed[mismatch]}; ${JSON.stringify(reported)}`);
        check(reported.length === 1 && reported[0][1] === 'injected slice failure', 'Warp failure was swallowed');
        engine.warpCtx.drawImage = draw;
        engine.reportRenderError = report;
        engine.mainRenderLoop(now += 17);
        cancelAnimationFrame(engine.animFrameId);
        check(engine.hasCompletedFrame, 'Rendering did not recover');
        return { modes: 29, strengths: 3, sampledFrames, transparentFrames: 0, untreatedFrames: 0, bufferResets, recovery: 'passed', offControls: 'passed' };
    } finally {
        cancelAnimationFrame(engine.animFrameId);
        canvas.remove();
    }
}
