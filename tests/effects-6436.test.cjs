'use strict';

// Proves the 6.4.36 effect banks ship untouched and that the engine registry is the hardcoded 6.4.36 set.
const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

const appDir = path.join(__dirname, '..', 'ai_warp_engine');

// LF-normalised SHA-256 of the sources exactly as committed in 6.4.36 (967be9c).
const BASELINE_6436 = {
    'glitch_engine.js': '5d8a18d078a5543ecd3a90c34f714fe85b741eee934281a944a38071cbef9650',
    'r3_gc3_factories.js': '67fe9840e64133584825cd1e73310349eb80d8909f77ec23493b0c9a80217448',
    'r3_ghost_vsync.js': '7138afb8cbaec5280f9fc613f28f5c8f1802dfc2558033dfb0ca8d544fef7167',
    'yt_lens.html': 'b32b8ffc1e874129cc75aa40e66036d021f7aa35054eac8751a6d1f391227334'
};

function lfSha256(file) {
    const text = fs.readFileSync(path.join(appDir, file)).toString('latin1').replace(/\r\n/g, '\n');
    return crypto.createHash('sha256').update(Buffer.from(text, 'latin1')).digest('hex');
}

function loadScript(file, window = {}) {
    const context = vm.createContext({ addEventListener() {}, removeEventListener() {}, console, ...window });
    context.window = context;
    vm.runInContext(fs.readFileSync(path.join(appDir, file), 'utf8'), context, { filename: file });
    return context;
}

// Values created inside a vm context have another realm's prototypes; compare them as plain JSON.
const plain = value => JSON.parse(JSON.stringify(value));

// Minimal DOM: enough for the real AiWarpEngine constructor to run without a browser.
function stubDom() {
    const context2d = () => new Proxy({}, {
        get(target, key) {
            if (key in target) return target[key];
            if (key === 'measureText') return () => ({ width: 8 });
            if (key === 'getImageData') return (x, y, w, h) => ({ data: new Uint8ClampedArray(Math.max(4, w * h * 4)) });
            if (/^create(Linear|Radial|Conic)?Gradient$|^createPattern$/.test(String(key))) return () => ({ addColorStop() {} });
            return () => undefined;
        },
        set(target, key, value) { target[key] = value; return true; }
    });
    const makeCanvas = () => {
        const ctx = context2d();
        const canvas = {
            width: 300, height: 150, clientWidth: 1280, clientHeight: 720, style: {},
            getContext: () => ctx, addEventListener() {}, removeEventListener() {}
        };
        ctx.canvas = canvas;
        return canvas;
    };
    const element = () => ({
        style: {}, classList: { add() {}, remove() {}, toggle() {}, contains: () => false },
        setAttribute() {}, appendChild() {}, addEventListener() {}, getContext: () => context2d()
    });
    const document = {
        createElement: tag => (tag === 'canvas' ? makeCanvas() : element()),
        getElementById: () => null, querySelector: () => null, querySelectorAll: () => [],
        body: element(), documentElement: element(), addEventListener() {}
    };
    return { document, makeCanvas, performance };
}

function loadRealEngine() {
    const dom = stubDom();
    const window = {
        document: dom.document, performance, console,
        innerWidth: 1280, innerHeight: 720, devicePixelRatio: 1,
        addEventListener() {}, removeEventListener() {},
        requestAnimationFrame: () => 1, cancelAnimationFrame() {},
        CustomEvent: class {}, dispatchEvent() {}
    };
    const context = vm.createContext(window);
    context.window = context;
    for (const file of ['glitch_engine.js', 'r3_gc3_factories.js', 'r3_ghost_vsync.js', 'ai_warp_engine.js']) {
        vm.runInContext(fs.readFileSync(path.join(appDir, file), 'utf8'), context, { filename: file });
    }
    return { context, canvas: dom.makeCanvas() };
}

test('the four 6.4.36 effect sources are byte-identical (LF-normalised) to the pinned baseline', () => {
    for (const [file, expected] of Object.entries(BASELINE_6436)) {
        assert.equal(lfSha256(file), expected, `${file} was modified after 6.4.36.`);
    }
});

test('the engine manifest pins the same bank hashes as this test', () => {
    const { AiWarpEngine } = loadScript('ai_warp_engine.js');
    assert.deepEqual(plain(AiWarpEngine.FX_MANIFEST.banks), BASELINE_6436);
    assert.equal(AiWarpEngine.FX_MANIFEST.baseline, '6.4.36');
    assert.ok(Object.isFrozen(AiWarpEngine.FX_MANIFEST) && Object.isFrozen(AiWarpEngine.FX_MANIFEST.voices.core),
        'The manifest must be deeply frozen.');
});

test('the manifest is exactly 17 core + 17 GC#(3) voices and 29 warp modes plus AI ZERO', () => {
    const { AiWarpEngine } = loadScript('ai_warp_engine.js');
    const { GC3FactoryBank } = loadScript('r3_gc3_factories.js');
    const manifest = AiWarpEngine.FX_MANIFEST;
    assert.equal(manifest.voices.core.length, 17);
    assert.equal(manifest.voices.gc3.length, 17);
    assert.equal(new Set(manifest.voices.core.concat(manifest.voices.gc3)).size, 34);
    assert.deepEqual(plain(manifest.voices.gc3), plain(GC3FactoryBank.voiceNames));
    assert.equal(manifest.warp.modes.length, 29);
    assert.equal(manifest.warp.modes.filter(mode => mode.startsWith('newblue_')).length, 17);
    assert.equal(manifest.warp.zero, 'ai_zero');
    assert.equal(manifest.milkdrop.presets, 80);
});

test('the real engine constructs with a registry that matches the manifest and runs all 34 voices', () => {
    const { context, canvas } = loadRealEngine();
    const engine = new context.AiWarpEngine(canvas);
    cancelEngineLoop(engine);
    assert.deepEqual(plain(engine.fxRegistryReport), {
        ok: true, baseline: '6.4.36', voices: 34, warpModes: 29, problems: []
    });
    assert.equal(engine.activeEffects.length, 34);
    assert.equal(engine.milkdropPresetDeck.length, 80);
    assert.ok(engine.r3GhostVsync, 'R3 Ghost Protocol / V-Sync rails are missing.');
});

test('a changed registry is reported but never throws or blocks the engine', () => {
    const { context, canvas } = loadRealEngine();
    const engine = new context.AiWarpEngine(canvas);
    cancelEngineLoop(engine);
    const warnings = [];
    const originalWarn = context.console.warn;
    context.console.warn = (...args) => warnings.push(args.join(' '));
    try {
        engine.coreEffectNames = engine.coreEffectNames.slice(1);
        engine.aiWarpModes = engine.aiWarpModes.concat('bogus');
        const report = engine.verifyFxRegistry();
        assert.equal(report.ok, false);
        assert.equal(report.problems.length, 2);
        assert.equal(warnings.length, 1);
    } finally {
        context.console.warn = originalWarn;
    }
});

test('the engine keeps every effect buffer the same size as the lens canvas, including Tech Artist', () => {
    const { context, canvas } = loadRealEngine();
    for (const [dpr, cssWidth, cssHeight] of [[1, 1920, 1080], [1.25, 1536, 864], [1.5, 1280, 720], [2, 1280, 720], [3, 800, 450]]) {
        context.devicePixelRatio = dpr;
        canvas.clientWidth = cssWidth;
        canvas.clientHeight = cssHeight;
        const engine = new context.AiWarpEngine(canvas);
        cancelEngineLoop(engine);
        const layout = plain(engine.getCanvasLayout());
        const scale = Math.min(Math.max(dpr, 1), 2);
        assert.deepEqual(layout.expected, { width: Math.round(cssWidth * scale), height: Math.round(cssHeight * scale) });
        assert.equal(layout.aligned, true, `Buffers misaligned at DPR ${dpr}: ${layout.mismatched}`);
        for (const [name, size] of Object.entries(layout.buffers)) {
            assert.deepEqual(size, layout.expected, `${name} buffer differs at DPR ${dpr}.`);
        }
    }
});

test('Tech Artist renders into a buffer that shares the resolution uniform and is scaled onto the lens', () => {
    const { context, canvas } = loadRealEngine();
    const created = [];
    const draws = [];
    context.THREE = fakeThree(created);
    for (const [dpr, cssWidth, cssHeight] of [[1, 1920, 1080], [1.5, 1280, 720], [2, 1920, 1080]]) {
        context.devicePixelRatio = dpr;
        canvas.clientWidth = cssWidth;
        canvas.clientHeight = cssHeight;
        canvas.width = 300;
        canvas.height = 150;
        const engine = new context.AiWarpEngine(canvas);
        cancelEngineLoop(engine);
        const shader = engine.initTechArtistShader();
        const expected = engine.getTechArtistBufferSize();
        assert.equal(shader.canvas.width, expected.width);
        assert.equal(shader.canvas.height, expected.height);
        assert.equal(shader.renderer.pixelRatio, 1, 'The WebGL pixel ratio must stay 1 so the buffer matches the uniform.');
        assert.equal(shader.uniforms.resolution.value.x, expected.width);
        assert.equal(shader.uniforms.resolution.value.y, expected.height);
        assert.ok(expected.width * expected.height <= 2560 * 1440 + 2560, 'Tech Artist buffer exceeds its pixel budget.');
        assert.ok(Math.abs(expected.width / expected.height - canvas.width / canvas.height) < 0.01, 'Tech Artist aspect differs.');

        // Resizing the window re-syncs the WebGL buffer with the uniform.
        canvas.clientWidth = Math.round(cssWidth / 2);
        canvas.clientHeight = Math.round(cssHeight / 2);
        engine.resize();
        const resized = engine.getTechArtistBufferSize();
        assert.equal(shader.canvas.width, resized.width);
        assert.equal(shader.uniforms.resolution.value.x, resized.width);
        assert.equal(engine.getCanvasLayout().aligned, true);

        // The voice must blit the whole WebGL buffer onto the whole lens canvas.
        const ctx = canvas.getContext('2d');
        ctx.drawImage = (source, ...args) => { if (source === shader.canvas) draws.push(args); };
        engine.setEffectEnabled('Tech Artist', true);
        engine.setEffectLevel('Tech Artist', 1);
        engine.effectAlphas['Tech Artist'] = 1;
        engine.effectEnabled['Tech Artist'] = true;
        engine.activeEffects[engine.coreEffectNames.indexOf('Tech Artist')](ctx, 1000);
        assert.deepEqual(draws.pop(), [0, 0, canvas.width, canvas.height], `Tech Artist blit is not full-canvas at DPR ${dpr}.`);
    }
});

test('resizing keeps the last completed frame instead of leaving a blank canvas', () => {
    const { context, canvas } = loadRealEngine();
    const engine = new context.AiWarpEngine(canvas);
    cancelEngineLoop(engine);
    const log = [];
    for (const target of ['canvas', 'completedFrameCanvas', 'genCanvas', 'milkdropFeedbackCanvas']) {
        const ctx = engine[target].getContext('2d');
        ctx.drawImage = (source, ...args) => log.push([target, source === engine.workCanvas ? 'scratch' : 'other', ...args]);
    }
    engine.hasCompletedFrame = true;
    canvas.clientWidth = 640;
    canvas.clientHeight = 360;
    assert.equal(engine.resize(), true);
    for (const target of ['canvas', 'completedFrameCanvas', 'genCanvas', 'milkdropFeedbackCanvas']) {
        assert.ok(log.some(([name, source, , , width, height]) => name === target && source === 'scratch' && width === 640 && height === 360),
            `${target} did not receive the scaled previous picture.`);
    }
    assert.equal(engine.hasCompletedFrame, true, 'The completed frame must survive a resize.');
    assert.equal(engine.resize(), false, 'An unchanged size must not touch any buffer.');
});

test('a failed WebGL start falls back to the 2D rays and is retried at most every 5 seconds', () => {
    const { context, canvas } = loadRealEngine();
    const engine = new context.AiWarpEngine(canvas);
    cancelEngineLoop(engine);
    const three = fakeThree([]);
    let attempts = 0;
    three.WebGLRenderer = class { constructor() { attempts++; throw new Error('WebGL is unavailable'); } };
    context.THREE = three;
    let now = 100000;
    context.performance = { now: () => now };
    const warnings = [];
    const originalWarn = context.console.warn;
    context.console.warn = (...args) => warnings.push(args.join(' '));
    try {
        const ctx = canvas.getContext('2d');
        let strokes = 0;
        ctx.stroke = () => { strokes++; };
        engine.setEffectEnabled('Tech Artist', true);
        engine.setEffectLevel('Tech Artist', 1);
        engine.effectAlphas['Tech Artist'] = 1;
        engine.effectEnabled['Tech Artist'] = true;
        const tech = engine.activeEffects[engine.coreEffectNames.indexOf('Tech Artist')];
        for (let frame = 0; frame < 120; frame++) { now += 16; tech(ctx, now); }
        assert.equal(attempts, 1, 'A failed WebGL start must not be retried on every frame.');
        assert.equal(warnings.length, 1, 'A failed WebGL start must warn once, not on every frame.');
        assert.ok(strokes > 0, 'The 2D fallback did not draw.');
        now += 5001;
        tech(ctx, now);
        assert.equal(attempts, 2, 'A failed WebGL start must be retried once the back-off has passed.');
    } finally {
        context.console.warn = originalWarn;
    }
});

function cancelEngineLoop(engine) {
    engine.animFrameId = 0;
}

// Just enough of THREE for the Tech Artist plumbing (the shader itself is exercised in the host test).
function fakeThree(created) {
    class Vector2 {
        constructor(x = 0, y = 0) { this.x = x; this.y = y; }
        set(x, y) { this.x = x; this.y = y; return this; }
    }
    class WebGLRenderer {
        constructor() {
            this.pixelRatio = 1;
            this.domElement = { width: 300, height: 150, style: {} };
            created.push(this);
        }
        setPixelRatio(value) { this.pixelRatio = value; }
        setSize(width, height, updateStyle) {
            this.domElement.width = Math.floor(width * this.pixelRatio);
            this.domElement.height = Math.floor(height * this.pixelRatio);
            if (updateStyle !== false) this.domElement.style.width = width + 'px';
        }
        render() {}
    }
    class Scene { add() {} }
    class OrthographicCamera {}
    class ShaderMaterial { constructor(options) { Object.assign(this, options); } }
    class PlaneGeometry {}
    class Mesh {}
    return { Vector2, WebGLRenderer, Scene, OrthographicCamera, ShaderMaterial, PlaneGeometry, Mesh };
}
