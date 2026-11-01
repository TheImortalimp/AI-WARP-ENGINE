'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const vm = require('node:vm');
const { test } = require('node:test');

function harness() {
    const classes = new Set();
    let listener;
    let posted;
    let timeout;
    const context = vm.createContext({
        document: { documentElement: { classList: {
            add: name => classes.add(name), remove: name => classes.delete(name)
        } } },
        requestAnimationFrame: callback => callback(),
        setTimeout: callback => { timeout = callback; return 1; },
        clearTimeout() {}
    });
    context.window = context;
    vm.runInContext(fs.readFileSync(require('node:path').join(__dirname, '..', 'ai_warp_engine', 'freeze_frame.js'), 'utf8'), context);
    const capture = new context.FreezeFrameCapture({
        addEventListener: (_, callback) => { listener = callback; },
        postMessage: value => { posted = value; }
    });
    return { capture, classes, posted: () => posted,
        reply: result => listener({ data: 'freeze:result|' + JSON.stringify(result) }),
        timeout: () => timeout() };
}

test('native capture masks HUD, waits for matching PNG and restores the page', async () => {
    const h = harness();
    const pending = h.capture.capture({ width: 16, height: 16 }, 'video');
    await Promise.resolve();
    assert.ok(h.classes.has('freeze-frame-capture'));
    assert.equal(h.posted(), 'freeze:capture|1');
    h.reply({ id: '999', png: 'AAAA' });
    assert.ok(h.capture.pending);
    h.reply({ id: '1', png: 'AAAA' });
    assert.equal(await pending, 'data:image/png;base64,AAAA');
    assert.equal(h.classes.size, 0);
    assert.equal(h.capture.busy, false);
});

test('native failure and timeout restore controls and allow a later capture', async () => {
    const h = harness();
    let pending = h.capture.capture({ width: 16, height: 16 });
    await Promise.resolve();
    h.reply({ id: '1', error: 'capture failed' });
    await assert.rejects(pending, /capture failed/);
    assert.equal(h.classes.size, 0);
    pending = h.capture.capture({ width: 16, height: 16 });
    await Promise.resolve();
    h.timeout();
    await assert.rejects(pending, /timed out/);
    assert.equal(h.classes.size, 0);
});

test('duplicate captures and malformed native output fail explicitly', async () => {
    const h = harness();
    const pending = h.capture.capture({ width: 16, height: 16 });
    await Promise.resolve();
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
