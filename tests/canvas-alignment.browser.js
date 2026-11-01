'use strict';

// Host-run proof that the LENS stage, every effect buffer, the Tech Artist shader and the fullscreen FX layer
// share one coordinate space at the current window size and at every device-pixel-ratio the engine supports.
// In the running native app: await runCanvasAlignmentChecks().
async function runCanvasAlignmentChecks() {
    const live = window.aiWarpEngine;
    if (!live) throw new Error('Engine not initialized.');
    const check = (condition, message) => { if (!condition) throw new Error(message); };
    const near = (a, b) => Math.abs(a - b) <= 1;
    const frames = async count => {
        for (let i = 0; i < count; i++) {
            await new Promise(resolve => {
                const timer = setTimeout(resolve, 100);
                requestAnimationFrame(() => { clearTimeout(timer); resolve(); });
            });
        }
    };
    const dprDescriptor = Object.getOwnPropertyDescriptor(window, 'devicePixelRatio');
    const setDpr = value => Object.defineProperty(window, 'devicePixelRatio', { configurable: true, get: () => value });
    const restoreDpr = () => {
        if (dprDescriptor) Object.defineProperty(window, 'devicePixelRatio', dprDescriptor);
        else delete window.devicePixelRatio;
    };
    const supportedDpr = value => Math.min(Math.max(value, 1), 2);
    const rectOf = element => {
        const r = element.getBoundingClientRect();
        return { x: r.left, y: r.top, width: r.width, height: r.height };
    };
    const sameRect = (a, b) => near(a.x, b.x) && near(a.y, b.y) && near(a.width, b.width) && near(a.height, b.height);
    const mismatch = layout => `${layout.mismatched.join(', ')} | buffers ${JSON.stringify(layout.buffers)} | expected ${JSON.stringify(layout.expected)}`;
    const coverage = surface => {
        const data = surface.getContext('2d').getImageData(0, 0, surface.width, surface.height).data;
        let painted = 0;
        for (let i = 3; i < data.length; i += 4) if (data[i] > 0) painted++;
        return painted / (data.length / 4);
    };

    // 1. The three full-window layers cover the same viewport rectangle, as the running app laid them out.
    const viewport = { x: 0, y: 0, width: window.innerWidth, height: window.innerHeight };
    const layers = {};
    for (const selector of ['#canvas-stage', '#glitchCanvas', '#fullscreen-fx-layer']) {
        layers[selector] = rectOf(document.querySelector(selector));
        check(sameRect(layers[selector], viewport),
            `${selector} is not aligned with the window: ${JSON.stringify(layers[selector])} vs ${JSON.stringify(viewport)}`);
    }
    // The YouTube shell deliberately bleeds 6% past every edge (inset -6%, 112%) so the warp bus never exposes the stage.
    const bleed = 0.06;
    const shell = document.getElementById('ytWarpShell');
    const shellDisplay = shell.style.display;
    shell.style.display = 'block';
    try {
        const expectedShell = {
            x: -bleed * viewport.width, y: -bleed * viewport.height,
            width: viewport.width * (1 + 2 * bleed), height: viewport.height * (1 + 2 * bleed)
        };
        check(sameRect(rectOf(shell), expectedShell),
            `The YouTube LENS shell is not centred on the effects canvas with its 6% bleed: ${JSON.stringify(rectOf(shell))} vs ${JSON.stringify(expectedShell)}`);
        check(sameRect(rectOf(document.getElementById('ytPlayerMount')), rectOf(shell)), 'The YouTube player mount does not fill the LENS shell.');
    } finally {
        shell.style.display = shellDisplay;
    }

    // 2. Every effect buffer of the live engine matches the lens canvas without any manual resize.
    const liveLayout = live.getCanvasLayout();
    check(liveLayout.aligned, `Live engine buffers are misaligned: ${mismatch(liveLayout)}`);
    check(near(liveLayout.css.width, viewport.width) && near(liveLayout.css.height, viewport.height), 'The LENS canvas does not fill the window.');
    check(live.fxRegistryReport && live.fxRegistryReport.ok, `The 6.4.36 FX registry check failed: ${JSON.stringify(live.fxRegistryReport)}`);

    // 3. A private engine on a small off-screen canvas is driven through the device-pixel-ratio matrix.
    const canvas = document.createElement('canvas');
    canvas.style.cssText = 'position:fixed;left:-2000px;top:0;width:320px;height:180px';
    document.body.appendChild(canvas);
    const engine = new window.AiWarpEngine(canvas);
    cancelAnimationFrame(engine.animFrameId);
    const source = document.createElement('canvas');
    source.width = 320;
    source.height = 180;
    const sourceContext = source.getContext('2d');
    for (let x = 0; x < source.width; x += 8) {
        sourceContext.fillStyle = x % 16 ? '#204080' : '#90b060';
        sourceContext.fillRect(x, 0, 8, source.height);
    }
    engine.setSourceMode('image', source);
    engine.setAiWarpActive(false);
    engine.setGhostMode('OFF', 0);
    engine.setVsyncMode('OFF', 0);
    for (const name of engine.effectNames) engine.setEffectEnabled(name, false);
    engine.setEffectEnabled('Tech Artist', true);
    engine.setEffectLevel('Tech Artist', 0.8);
    let now = 100000;
    const render = label => {
        engine.mainRenderLoop(now += 100);
        cancelAnimationFrame(engine.animFrameId);
        check(engine.renderErrors.size === 0, `${label}: ${JSON.stringify([...engine.renderErrors])}`);
    };
    const matrix = [];
    try {
        render('first frame');
        const webgl = !!(engine.techArtistShader && engine.techArtistShader.renderer);
        for (const dpr of [1, 1.25, 1.5, 2, 3]) {
            setDpr(dpr);
            engine.resize();
            render(`DPR ${dpr}`);
            const layout = engine.getCanvasLayout();
            const scale = supportedDpr(dpr);
            check(layout.aligned, `DPR ${dpr}: misaligned buffers ${mismatch(layout)}`);
            check(layout.buffers.main.width === Math.round(layout.css.width * scale)
                && layout.buffers.main.height === Math.round(layout.css.height * scale),
                `DPR ${dpr}: the lens canvas is not the CSS size x ${scale}.`);
            if (webgl) {
                const shader = layout.techArtist;
                check(shader && shader.width === shader.resolution.width && shader.height === shader.resolution.height,
                    `DPR ${dpr}: the Tech Artist buffer and its resolution uniform differ.`);
                check(shader.width === shader.expected.width && shader.height === shader.expected.height,
                    `DPR ${dpr}: the Tech Artist buffer is not the lens size.`);
            }
            matrix.push({ dpr, main: layout.buffers.main, techArtist: webgl ? { width: layout.techArtist.width, height: layout.techArtist.height } : '2d-fallback' });
        }
        restoreDpr();
        engine.resize();

        // 4. A resize keeps the last picture on screen (no blank frame) and re-aligns every buffer.
        render('pre-resize frame');
        check(coverage(engine.canvas) > 0.99 && coverage(engine.completedFrameCanvas) > 0.99, 'No picture before the resize test.');
        canvas.style.width = '400px';
        canvas.style.height = '225px';
        check(engine.resize() === true, 'resize() did not report the size change.');
        check(coverage(engine.canvas) > 0.99, 'The lens canvas went blank after a resize.');
        check(coverage(engine.completedFrameCanvas) > 0.99, 'The last completed frame was lost by a resize.');
        const resized = engine.getCanvasLayout();
        check(resized.aligned, `After resize: ${mismatch(resized)}`);

        // 5. The ResizeObserver path (what a real window drag uses) aligns and preserves the picture too.
        canvas.style.width = '260px';
        canvas.style.height = '146px';
        await frames(3);
        const observed = engine.getCanvasLayout();
        check(observed.aligned && observed.buffers.main.width === Math.round(260 * supportedDpr(window.devicePixelRatio || 1)),
            `The ResizeObserver did not re-align the buffers: ${mismatch(observed)}`);
        check(coverage(engine.canvas) > 0.99, 'The lens canvas went blank after an observed resize.');

        return {
            result: 'passed',
            window: { width: viewport.width, height: viewport.height, dpr: window.devicePixelRatio },
            layers, live: { css: liveLayout.css, main: liveLayout.buffers.main, techArtist: liveLayout.techArtist },
            techArtistMode: webgl ? 'webgl' : '2d-fallback', matrix,
            resize: { explicit: resized.buffers.main, observed: observed.buffers.main }
        };
    } finally {
        cancelAnimationFrame(engine.animFrameId);
        restoreDpr();
        const shader = engine.techArtistShader;
        if (shader && shader.renderer) {
            shader.renderer.dispose();
            if (typeof shader.renderer.forceContextLoss === 'function') shader.renderer.forceContextLoss();
        }
        canvas.remove();
    }
}

window.runCanvasAlignmentChecks = runCanvasAlignmentChecks;
