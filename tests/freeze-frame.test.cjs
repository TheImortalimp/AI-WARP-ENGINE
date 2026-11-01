'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

const settle = async () => { for (let i = 0; i < 60; i++) await Promise.resolve(); };

function harness(hudNodes = []) {
    const classes = new Set();
    let listener;
    let posted;
    let timeout;
    let frames = 0;
    const context = vm.createContext({
        document: {
            documentElement: { classList: {
                add: name => classes.add(name), remove: name => classes.delete(name)
            } },
            querySelectorAll: () => hudNodes
        },
        getComputedStyle: node => ({ visibility: node.visibility(frames) }),
        requestAnimationFrame: callback => { frames++; callback(); },
        setTimeout: callback => { timeout = callback; return 1; },
        clearTimeout() {}
    });
    context.window = context;
    vm.runInContext(fs.readFileSync(path.join(__dirname, '..', 'ai_warp_engine', 'freeze_frame.js'), 'utf8'), context);
    const capture = new context.FreezeFrameCapture({
        addEventListener: (_, callback) => { listener = callback; },
        postMessage: value => { posted = value; }
    });
    return { capture, classes, posted: () => posted, frames: () => frames,
        FreezeFrameCapture: context.FreezeFrameCapture,
        reply: result => listener({ data: 'freeze:result|' + JSON.stringify(result) }),
        timeout: () => timeout() };
}

test('native capture masks HUD, waits for matching PNG and restores the page', async () => {
    const h = harness();
    const pending = h.capture.capture({ width: 16, height: 16 }, 'video');
    await settle();
    assert.ok(h.classes.has('freeze-frame-capture'));
    assert.equal(h.posted(), 'freeze:capture|1');
    h.reply({ id: '999', png: 'AAAA' });
    assert.ok(h.capture.pending);
    h.reply({ id: '1', png: 'AAAA' });
    assert.equal(await pending, 'data:image/png;base64,AAAA');
    assert.equal(h.classes.size, 0);
    assert.equal(h.capture.busy, false);
});

test('native capture does not screenshot until every HUD node is computed hidden', async () => {
    // Models "transition: all" nodes that stay painted for several frames after the mask is applied.
    const slow = { visibility: frames => (frames >= 9 ? 'hidden' : 'visible') };
    const quick = { visibility: () => 'hidden' };
    const h = harness([quick, slow]);
    const pending = h.capture.capture({ width: 16, height: 16 }, 'video');
    await Promise.resolve();
    assert.equal(h.posted(), undefined);
    await settle();
    assert.equal(h.posted(), 'freeze:capture|1');
    assert.ok(h.frames() >= 9, 'The screenshot was requested before the slow HUD node was hidden.');
    h.reply({ id: '1', png: 'AAAA' });
    await pending;
});

test('a HUD node that never hides cannot hang the capture', async () => {
    const h = harness([{ visibility: () => 'visible' }]);
    const pending = h.capture.capture({ width: 16, height: 16 }, 'video');
    await settle();
    assert.equal(h.posted(), 'freeze:capture|1');
    h.reply({ id: '1', png: 'AAAA' });
    await pending;
    assert.equal(h.classes.size, 0);
});

test('JS HUD selector and CSS capture mask stay in sync and kill transitions', () => {
    const { FreezeFrameCapture } = harness();
    const css = fs.readFileSync(path.join(__dirname, '..', 'ai_warp_engine', 'media_page.css'), 'utf8');
    const rule = css.match(/(\.freeze-frame-capture[^{]+)\{([^}]*)\}/);
    assert.ok(rule, 'Capture mask rule is missing.');
    const selectors = rule[1].split(',').map(value => value.trim().replace(/^\.freeze-frame-capture\s+/, ''));
    assert.deepEqual(selectors, FreezeFrameCapture.HUD_SELECTOR.split(',').map(value => value.trim()));
    for (const declaration of ['visibility: hidden !important', 'transition: none !important', 'animation: none !important'])
        assert.ok(rule[2].includes(declaration), `Capture mask lacks ${declaration}`);
});

test('native failure and timeout restore controls and allow a later capture', async () => {
    const h = harness();
    let pending = h.capture.capture({ width: 16, height: 16 });
    await settle();
    h.reply({ id: '1', error: 'capture failed' });
    await assert.rejects(pending, /capture failed/);
    assert.equal(h.classes.size, 0);
    pending = h.capture.capture({ width: 16, height: 16 });
    await settle();
    h.timeout();
    await assert.rejects(pending, /timed out/);
    assert.equal(h.classes.size, 0);
});

test('duplicate captures and malformed native output fail explicitly', async () => {
    const h = harness();
    const pending = h.capture.capture({ width: 16, height: 16 });
    await settle();
    await assert.rejects(h.capture.capture({ width: 16, height: 16 }), /already in progress/);
    h.reply({ id: '1', png: 'not a PNG data string!' });
    await assert.rejects(pending, /Invalid native screenshot/);
    assert.equal(h.classes.size, 0);
});

test('browser YouTube requires readable capture instead of a partial screenshot', async () => {
    const h = harness();
    h.capture.bridge = null;
    await assert.rejects(h.capture.capture({ width: 16, height: 16 }, 'youtube'), /Share the YouTube/);
});
