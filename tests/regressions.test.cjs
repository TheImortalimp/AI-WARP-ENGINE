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
    'glitch_engine.js', 'r3_gc3_factories.js', 'r3_ghost_vsync.js',
    'ai_warp_engine.js', 'media_page.js'
];

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
        for (const external of scripts.filter(src => src.startsWith('https:'))) {
            assert.ok(html.includes(`<script async src="${external}"`), 'Remote scripts must not block core startup');
        }
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

test('a transient render failure cannot cancel subsequent animation frames', () => {
    const { engine, scheduled } = engineHarness();
    engine.renderGenerativeSource = () => { throw new Error('source not ready'); };
    assert.throws(() => engine.mainRenderLoop(116), /source not ready/);
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
    assert.deepEqual(balance(), [4, 4]);
    assert.equal(logged.length, 1);
    assert.equal(dispatched.length, 1);
    assert.equal(dispatched[0].detail.stage, 'voice 1');
    engine.activeEffects[0] = () => {};
    engine.mainRenderLoop(148);
    assert.equal(engine.renderErrors.size, 0);
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

test('HUD change notice, shell version, installer version and asset cache versions agree', () => {
    const project = fs.readFileSync(path.join(root, 'shell', 'AiWarpEngine.csproj'), 'utf8');
    const version = project.match(/<Version>([^<]+)<\/Version>/)[1];
    const installer = fs.readFileSync(path.join(root, 'shell', 'setup.nsi'), 'utf8');
    assert.ok(installer.includes(`!define VERSION "${version}"`));
    assert.ok(JSON.parse(read('media_page.json')).engine.version.startsWith(version + '-'));
    assert.ok(read('ai_warp_showcase_section.html').includes(`/releases/download/v${version}/AI-WARP-ENGINE-${version}-x64-setup.exe`));
    for (const name of aliases) {
        const html = read(name);
        assert.ok(html.includes(`v${version} · INIT + FX RESTORED · CURSOR GUARD`));
        const assets = [...html.matchAll(/(?:src|href)="([^"]+\.(?:js|css)\?v=([^"]+))"/g)];
        assert.equal(assets.length, 6);
        assert.ok(assets.every(match => match[2] === version + '.0'));
    }
});
