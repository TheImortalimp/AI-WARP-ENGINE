'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

function harness(withBridge = true) {
    const requests = [];
    const statuses = [];
    const revoked = [];
    let receive;
    const bridge = {
        addEventListener(name, callback) { receive = callback; },
        postMessage(message) { requests.push(message); },
        postMessageWithAdditionalObjects(message, objects) { requests.push([message, objects]); }
    };
    const context = vm.createContext({
        DOMException,
        URL: { createObjectURL: () => 'blob:original', revokeObjectURL: url => revoked.push(url) }
    });
    context.window = context;
    vm.runInContext(fs.readFileSync(path.join(__dirname, '..', 'ai_warp_engine', 'local_media.js'), 'utf8'), context);
    const player = new context.LocalMediaPlayback(withBridge ? bridge : null, message => statuses.push(message), () => {});
    const played = [];
    const element = {
        pause() {}, removeAttribute() {}, load() {},
        play() { played.push(this.src); return Promise.resolve(); }
    };
    const result = data => receive({ data: 'media:result|' + JSON.stringify(data) });
    return { player, element, requests, statuses, revoked, played, result };
}

const tick = () => new Promise(resolve => setImmediate(resolve));
const unsupported = () => new DOMException('Unsupported codec.', 'NotSupportedError');
const convertedUrl = 'https://media.warp.local/' + 'a'.repeat(64) + '.mp4';

test('supported local media plays directly without conversion and releases its blob on source switch', async () => {
    const { player, element, requests, revoked, played } = harness();
    await player.play(element, { name: 'native.mp4' }, 'video');
    assert.deepEqual(played, ['blob:original']);
    assert.equal(requests.length, 0);
    assert.equal(element.crossOrigin, 'anonymous');
    player.cancel();
    assert.deepEqual(revoked, ['blob:original']);
});

test('unsupported AVI converts once using the selected File object, then plays with CORS enabled', async () => {
    const { player, element, requests, statuses, result } = harness();
    const file = { name: 'clip.avi' };
    let calls = 0;
    element.play = () => ++calls === 1 ? Promise.reject(unsupported()) : Promise.resolve();
    const playback = player.play(element, file, 'video');
    await tick();
    assert.equal(requests.length, 1);
    assert.deepEqual(JSON.parse(requests[0][0].substring(14)), { id: '1', kind: 'video' });
    assert.equal(requests[0][1][0], file);
    assert.match(statuses[0], /CONVERTING LOCALLY/);
    result({ id: '1', url: convertedUrl });
    await playback;
    assert.equal(calls, 2);
    assert.equal(element.src, convertedUrl);
    assert.equal(element.crossOrigin, 'anonymous');
});

test('decoder error events also trigger conversion, including unsupported audio', async () => {
    const { player, element, requests, result } = harness();
    let calls = 0;
    element.play = () => {
        if (++calls === 1) {
            element.error = { code: 3, message: 'Cannot decode AIFF' };
            queueMicrotask(() => element.onerror());
            return new Promise(() => {});
        }
        return Promise.resolve();
    };
    const playback = player.play(element, { name: 'track.aiff' }, 'audio');
    await tick();
    assert.equal(JSON.parse(requests[0][0].substring(14)).kind, 'audio');
    result({ id: '1', url: convertedUrl.replace('.mp4', '.m4a') });
    await playback;
    assert.equal(calls, 2);
});

test('switching sources cancels conversion and stale replies cannot restart the old file', async () => {
    const { player, element, requests, result } = harness();
    element.play = () => Promise.reject(unsupported());
    const playback = player.play(element, { name: 'old.avi' }, 'video');
    const rejected = assert.rejects(playback, error => error.name === 'AbortError');
    await tick();
    player.cancel();
    result({ id: '1', url: convertedUrl });
    await rejected;
    assert.equal(requests[1], 'media:cancel|1');
    assert.equal(player.pending, null);
});

test('conversion failures and unsafe output URLs fail explicitly', async () => {
    for (const response of [
        { id: '1', error: 'FFmpeg: corrupt source' },
        { id: '1', url: 'https://remote.invalid/private.mp4' }
    ]) {
        const { player, element, result } = harness();
        element.play = () => Promise.reject(unsupported());
        const playback = player.play(element, { name: 'broken.avi' }, 'video');
        const rejected = assert.rejects(playback, /corrupt source|Invalid converted media URL/);
        await tick();
        result(response);
        await rejected;
    }
});

test('normal browser playback reports codec limits instead of pretending AVI is supported', async () => {
    const { player, element } = harness(false);
    element.play = () => Promise.reject(unsupported());
    await assert.rejects(player.play(element, { name: 'clip.avi' }, 'video'), /Convert the file locally/);
});

test('autoplay denial does not trigger unnecessary conversion', async () => {
    const { player, element, requests } = harness();
    element.play = () => Promise.reject(new DOMException('User gesture required.', 'NotAllowedError'));
    await assert.rejects(player.play(element, { name: 'clip.mp4' }, 'video'), error => error.name === 'NotAllowedError');
    assert.equal(requests.length, 0);
});
