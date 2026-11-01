'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

const root = path.resolve(__dirname, '..');
const app = path.join(root, 'ai_warp_engine');
const read = name => fs.readFileSync(path.join(app, name), 'utf8');
const aliases = ['index.html', 'ai_warp_engine.html', 'media_page.html'];
const dependencies = [
    'three.min.js',
    'glitch_engine.js', 'r3_gc3_factories.js', 'r3_ghost_vsync.js',
    'ai_warp_engine.js', 'local_media.js', 'freeze_frame.js', 'media_page.js'
];

test('native installer retains gold icon wiring and presents .NET licensing tribute', () => {
    const shell = name => fs.readFileSync(path.join(root, 'shell', name), 'utf8');
    const setup = shell('setup.nsi');
    assert.match(shell('AiWarpEngine.csproj'), /<ApplicationIcon>app\.ico<\/ApplicationIcon>/);
    for (const directive of ['Icon', 'UninstallIcon']) {
        assert.ok(setup.includes(`${directive} "app.ico"`));
    }
    for (const define of ['MUI_ICON', 'MUI_UNICON']) {
        assert.ok(setup.includes(`!define ${define} "app.ico"`));
    }
    assert.ok(setup.includes('!insertmacro MUI_PAGE_LICENSE "${SRC}\\Microsoft.NETCore.App.Runtime.win-x64-LICENSE.TXT.txt"'));
    assert.match(shell('LICENSE.txt'), /TRIBUTE TO MICROSOFT \.NET/);
    assert.match(shell('LICENSE.txt'), /does not imply Microsoft sponsorship or endorsement/);
});

test('application aliases contain one document, unique IDs and one ordered engine stack', () => {
    for (const name of aliases) {
        const html = read(name);
        assert.equal(html, read(aliases[0]), name);
        for (const tag of ['<!DOCTYPE html>', '<html ', '<body>', '</html>']) {
            assert.equal(html.split(tag).length - 1, 1, `${name}: ${tag}`);
        }
        assert.match(html, /<\/html>\s*$/);
        const ids = [...html.matchAll(/\bid="([^"]+)"/g)].map(match => match[1]);
        assert.equal(ids.length, new Set(ids).size, `${name}: duplicate IDs`);
        const scripts = [...html.matchAll(/<script\b[^>]*\bsrc="([^"]+)"/g)].map(match => match[1]);
        assert.deepEqual(scripts.filter(src => !src.startsWith('https:')).map(src => src.split('?')[0]), dependencies);
        assert.ok(!scripts.some(src => /^https?:/.test(src)), 'Main app must not execute remote scripts');
        assert.ok(html.includes("script-src 'self'; object-src 'none'; base-uri 'none'; form-action 'none'"));
    }
});

test('the showcase is only a fragment and the lens is only a player', () => {
    const showcase = read('ai_warp_showcase_section.html');
    assert.ok(!/<(?:html|body)\b/i.test(showcase));
    assert.ok(!dependencies.some(name => showcase.includes(`<script src="${name}`)));
    assert.ok(!showcase.includes('id="glitchCanvas"'));
    const lens = read('yt_lens.html');
    assert.equal((lens.match(/<!DOCTYPE html>/g) || []).length, 1);
    assert.ok(!lens.includes('glitchCanvas'));
    assert.ok(!lens.includes('media_page.js'));
});

test('all application/web JavaScript and inline scripts parse', () => {
    for (const folder of [app, path.join(root, 'web')]) {
        for (const name of fs.readdirSync(folder)) {
            if (name.endsWith('.js')) {
                new vm.Script(fs.readFileSync(path.join(folder, name), 'utf8'), { filename: name });
            }
            if (name.endsWith('.html')) {
                const html = fs.readFileSync(path.join(folder, name), 'utf8');
                for (const match of html.matchAll(/<script\b([^>]*)>([\s\S]*?)<\/script>/g)) {
                    if (!/\bsrc=/.test(match[1]) && !/\btype=["']application\/ld\+json/.test(match[1])) {
                        new vm.Script(match[2], { filename: name });
                    }
                }
            }
        }
    }
});

function engineHarness() {
    const scheduled = [];
    const logged = [];
    const dispatched = [];
    const context = vm.createContext({
        console: { error: (...args) => logged.push(args) },
        requestAnimationFrame: callback => { scheduled.push(callback); return scheduled.length; },
        CustomEvent: class { constructor(type, options) { this.type = type; this.detail = options.detail; } }
    });
    context.window = context;
    context.dispatchEvent = event => dispatched.push(event);
    vm.runInContext(read('ai_warp_engine.js'), context);
    const engine = Object.create(context.AiWarpEngine.prototype);
    let saves = 0;
    let restores = 0;
    Object.assign(engine, {
        canvas: { width: 16, height: 16 },
        gctx: { clearRect() {}, drawImage() {}, save() { saves++; }, restore() { restores++; } },
        sourceMode: 'generative', renderGenerativeSource() {},
        lastFrameTime: 100, activeEffects: [], renderErrors: new Map(),
        warpTime: 0, warpClockAt: null,
        completedFrameCtx: { clearRect() {}, drawImage() {} },
        completedFrameCanvas: {}, hasCompletedFrame: false,
        enforceLocks() {}, renderVsync() {}, renderGhostProtocol() {},
        renderAiWarp() {}, renderMilkdrop() {}, renderTransientEffects() {},
        updateCanvasVoiceFilter() {}
    });
    return { context, engine, scheduled, logged, dispatched, balance: () => [saves, restores] };
}

test('invalid canvases fail explicitly instead of returning a partial engine', () => {
    const { context } = engineHarness();
    assert.throws(() => new context.AiWarpEngine({}), /Canvas/);
    assert.throws(() => new context.AiWarpEngine({ getContext: () => null }), /2D rendering is unavailable/);
});

test('a transient render failure is reported and cannot cancel subsequent animation frames', () => {
    const { engine, scheduled, logged } = engineHarness();
    engine.renderGenerativeSource = () => { throw new Error('source not ready'); };
    assert.doesNotThrow(() => engine.mainRenderLoop(116));
    assert.equal(logged.length, 1);
    assert.equal(scheduled.length, 1);
    engine.renderGenerativeSource = () => {};
    scheduled[0](132);
    assert.equal(engine.lastFrameTime, 132);
    assert.equal(scheduled.length, 2);
});

test('voice errors restore canvas state, report once and do not block later voices', () => {
    const { engine, logged, dispatched, balance } = engineHarness();
    let laterVoiceCalls = 0;
    engine.activeEffects = [
        () => { throw new Error('broken voice'); },
        () => { laterVoiceCalls++; }
    ];
    engine.mainRenderLoop(116);
    engine.mainRenderLoop(132);
    assert.equal(laterVoiceCalls, 2);
    assert.deepEqual(balance(), [6, 6]);
    assert.equal(logged.length, 1);
    assert.equal(dispatched.length, 1);
    assert.equal(dispatched[0].detail.stage, 'voice 1');
    engine.activeEffects[0] = () => {};
    engine.mainRenderLoop(148);
    assert.equal(engine.renderErrors.size, 0);
});

test('failed system passes restore the last completed FX frame and recover on the next frame', () => {
    const { engine, scheduled, logged } = engineHarness();
    const draws = [];
    engine.gctx.drawImage = image => draws.push(image);
    engine.mainRenderLoop(116);
    assert.equal(engine.hasCompletedFrame, true);
    engine.renderAiWarp = () => { throw new Error('warp pass failed'); };
    engine.mainRenderLoop(132);
    engine.mainRenderLoop(148);
    assert.equal(draws.at(-1), engine.completedFrameCanvas);
    assert.equal(logged.length, 1);
    assert.equal(scheduled.length, 3);
    engine.renderAiWarp = () => {};
    engine.mainRenderLoop(164);
    assert.equal(engine.renderErrors.size, 0);
});

test('warp phase integrates speed changes without rescaling elapsed time', () => {
    const { engine } = engineHarness();
    engine.glitchSpeed = 1;
    assert.equal(engine.getWarpTime(100000), 0);
    assert.ok(Math.abs(engine.getWarpTime(100016) - 0.016) < 1e-12);
    engine.glitchSpeed = 2;
    assert.ok(Math.abs(engine.getWarpTime(100032) - 0.048) < 1e-12);
    assert.equal(engine.getWarpTime(100032), 0.048);
    assert.ok(Math.abs(engine.getWarpTime(200000) - 0.248) < 1e-12);
    assert.equal(engine.getWarpTime(199999), 0.248);
});

test('buffering video keeps a clean source frame under continuing FX instead of feeding FX back into themselves', () => {
    const { engine } = engineHarness();
    const source = { readyState: 2, videoWidth: 16, videoHeight: 16 };
    let captures = 0;
    let frames = 0;
    engine.sourceMode = 'video';
    engine.activeVideoElement = source;
    engine.genCanvas = {};
    engine.genCtx = { save() {}, restore() {}, drawImage(image) { assert.equal(image, source); captures++; } };
    engine.gctx.drawImage = image => { assert.equal(image, engine.genCanvas); frames++; };
    engine.renderFrame(116, 0.016);
    source.readyState = 1;
    engine.renderFrame(132, 0.016);
    engine.renderFrame(148, 0.016);
    assert.equal(captures, 1);
    assert.equal(frames, 3);
});

function warpHarness() {
    const { engine } = engineHarness();
    delete engine.renderAiWarp;
    const draws = [];
    const ctx = {
        clearRect() {}, save() {}, restore() {},
        drawImage(image) { draws.push([image, this.globalAlpha]); }
    };
    Object.assign(engine, {
        aiWarpActive: true, aiWarpStrength: 0.65, aiWarpMode: 'liquid',
        warpRenderedMode: null, warpTransitionAt: null, warpTransitionDuration: 240,
        workCanvas: {}, workCtx: ctx, warpCanvas: {}, warpCtx: ctx,
        warpTransitionCanvas: {}, warpTransitionCtx: ctx,
        renderAiWarpFrame() {}
    });
    return { engine, draws };
}

test('mode changes crossfade the outgoing FX without a zero-strength frame, including rapid retargets', () => {
    const { engine, draws } = warpHarness();
    engine.renderAiWarp(100);
    engine.aiWarpMode = 'prism';
    engine.renderAiWarp(116);
    assert.equal(engine.warpRenderedMode, 'prism');
    assert.equal(engine.warpTransitionAt, 116);
    assert.equal(draws.filter(([image]) => image === engine.warpTransitionCanvas).at(-1)[1], 1);
    engine.renderAiWarp(236);
    assert.equal(draws.filter(([image]) => image === engine.warpTransitionCanvas).at(-1)[1], 0.5);
    engine.aiWarpMode = 'vortex';
    engine.renderAiWarp(252);
    assert.equal(engine.warpTransitionAt, 252);
    engine.renderAiWarp(492);
    assert.equal(engine.warpTransitionAt, null);
});

test('AI ZERO and OFF cancel retained transitions immediately', () => {
    for (const off of ['ai_zero', 'inactive', 'strength']) {
        const { engine, draws } = warpHarness();
        engine.renderAiWarp(100);
        engine.aiWarpMode = 'prism';
        engine.renderAiWarp(116);
        if (off === 'ai_zero') engine.aiWarpMode = 'ai_zero';
        if (off === 'inactive') engine.aiWarpActive = false;
        if (off === 'strength') engine.aiWarpStrength = 0;
        const before = draws.length;
        engine.renderAiWarp(132);
        assert.equal(draws.length, before);
        assert.equal(engine.warpRenderedMode, null);
        assert.equal(engine.warpTransitionAt, null);
    }
});

test('a failed warp pass never copies its partial output to the lens', () => {
    const { engine } = warpHarness();
    let published = false;
    engine.gctx.drawImage = () => { published = true; };
    engine.renderAiWarpFrame = () => { throw new Error('slice draw failed'); };
    assert.throws(() => engine.renderAiWarp(100), /slice draw failed/);
    assert.equal(published, false);
    assert.equal(engine.warpRenderedMode, null);
});

test('background-tab resume clamps automation delta', () => {
    const { engine } = engineHarness();
    engine.isAutoGlitch = true;
    let dt;
    engine.updateAutomation = (now, delta) => { dt = delta; };
    engine.mainRenderLoop(100000);
    assert.equal(dt, 0.1);
    engine.mainRenderLoop(99999);
    assert.equal(dt, 0);
});

test('duplicate integration scripts claim initialization only once and show missing-dependency errors', async () => {
    const callbacks = [];
    const banner = { classList: { add() {} }, setAttribute() {} };
    const element = { dataset: {} };
    const errors = [];
    const context = vm.createContext({
        document: {
            documentElement: element,
            addEventListener: (name, callback) => callbacks.push(callback),
            getElementById: () => banner
        },
        console: { error: (...args) => errors.push(args) },
        clearTimeout() {}, addEventListener() {}
    });
    context.window = context;
    vm.runInContext(read('media_page.js'), context);
    vm.runInContext(read('media_page.js'), context);
    callbacks.forEach(callback => callback());
    await new Promise(resolve => setImmediate(resolve));
    assert.equal(element.dataset.warpStartup, 'failed');
    assert.equal(errors.length, 1);
    assert.match(banner.textContent, /STARTUP FAILED.*GlitchEngine/);
});

test('lens URL validation accepts only exact HTTPS YouTube embed origins and paths', () => {
    const script = [...read('yt_lens.html').matchAll(/<script>([\s\S]*?)<\/script>/g)][0][1];
    for (const [src, allowed] of [
        ['https://www.youtube.com/embed/test?autoplay=1', true],
        ['https://www.youtube-nocookie.com/embed?list=PLtest', true],
        ['https://www.youtube.com/embedder/test', false],
        ['https://www.youtube.com.evil.invalid/embed/test', false],
        ['https://www.youtube.com:444/embed/test', false],
        ['https://user@www.youtube.com/embed/test', false],
        ['http://www.youtube.com/embed/test', false],
        ['javascript:alert(1)', false],
        ['', false]
    ]) {
        const frames = [];
        const body = { appendChild: frame => frames.push(frame), textContent: '' };
        vm.runInNewContext(script, {
            URL, URLSearchParams,
            location: { search: '?src=' + encodeURIComponent(src) },
            document: { body, createElement: () => ({}) },
            console: { error() {} }
        });
        assert.equal(frames.length, allowed ? 1 : 0, src);
        if (!allowed) assert.match(body.textContent, /invalid or unsupported/);
    }
});

test('cursor page guard still parses and is attached to both native views', () => {
    const guard = fs.readFileSync(path.join(root, 'shell', 'CursorGuard.cs'), 'utf8');
    const script = guard.match(/private const string PageScript = """\r?\n([\s\S]*?)\r?\n""";/)[1];
    new vm.Script(script);
    const host = fs.readFileSync(path.join(root, 'shell', 'Program.cs'), 'utf8');
    assert.ok(host.includes('await CursorGuard.AttachAsync(core);'));
    assert.ok(host.includes('await CursorGuard.AttachAsync(lensView.CoreWebView2);'));
    assert.ok(host.includes('CursorGuard.HardenBrowser(core);'));
    assert.ok(host.includes('CursorGuard.HardenBrowser(lensView.CoreWebView2);'));
});

test('both native views enforce document boundaries and release debugging is compiled out', () => {
    const host = fs.readFileSync(path.join(root, 'shell', 'Program.cs'), 'utf8');
    assert.match(host, /#if DEBUG[\s\S]*?AIWARP_DEBUG_PORT[\s\S]*?#else[\s\S]*?HasBrowserControlOverride\(\)[\s\S]*?#endif/);
    assert.ok(host.includes('if (!AppControlPolicy.IsAppDocument(e.Source))'));
    assert.ok(host.includes('if (!AppControlPolicy.IsAppDocument(e.Uri))'));
    assert.ok(host.includes('AppControlPolicy.IsAppDocument(e.Uri, lens: true)'));
    assert.ok(!host.includes('UseShellExecute = true'));
    assert.ok(host.includes('Opening external browsers or new windows is disabled.'));
    assert.ok(host.includes('lensView.CoreWebView2.Settings.IsWebMessageEnabled = false;'));
    assert.equal((host.match(/Settings.AreHostObjectsAllowed = false;/g) || []).length, 2);
    assert.ok(host.includes('CoreWebView2PermissionState.Deny'));
});

test('HUD change notice, shell version, installer version and asset cache versions agree', () => {
    const project = fs.readFileSync(path.join(root, 'shell', 'AiWarpEngine.csproj'), 'utf8');
    const version = project.match(/<Version>([^<]+)<\/Version>/)[1];
    const installer = fs.readFileSync(path.join(root, 'shell', 'setup.nsi'), 'utf8');
    assert.ok(installer.includes(`!define VERSION "${version}"`));
    assert.ok(JSON.parse(read('media_page.json')).engine.version.startsWith(version + '-'));
    assert.ok(read('ai_warp_showcase_section.html').includes(`${version} COMPLETE (ALL WINDOWS x64 NATIVE) -noffmpeg_(~Gyan-D)`));
    for (const name of aliases) {
        const html = read(name);
        assert.ok(html.includes(`${version} COMPLETE (ALL WINDOWS x64 NATIVE) -noffmpeg_(~Gyan-D)`));
        const assets = [...html.matchAll(/(?:src|href)="([^"]+\.(?:js|css)\?v=([^"]+))"/g)];
        assert.equal(assets.length, 9);
        assert.ok(assets.every(match => match[2] === version));
    }
});
