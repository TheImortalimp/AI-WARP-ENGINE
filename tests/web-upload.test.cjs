'use strict';

const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

const root = path.resolve(__dirname, '..');
const pages = ['home.html', 'index.html', 'ai_warp_engine/ai_warp_showcase_section.html'];

function showcase(html) {
    const start = html.indexOf('<section id="ai-warp-engine-showcase"');
    assert.ok(start >= 0);
    const end = html.indexOf('</script>', html.indexOf('</section>', start));
    assert.ok(end > start);
    return html.slice(start, end + 9);
}

test('website showcases have current branding, valid local paths and accurate download status', () => {
    for (const file of pages) {
        const html = showcase(fs.readFileSync(path.join(root, file), 'utf8'));
        assert.match(html, /6\.4\.38\.2 NO-FFMPEG \(No~Gyan-D\)/);
        assert.match(html, /Browser AVI requires local conversion/);
        assert.match(html, /Windows-only/);
        assert.match(html, /Full FX Freeze Frame \(6\.4\.38\.2\)/);
        assert.doesNotMatch(html, /releases\/(?:download|tag)\/v6\.4\.(?:36|37)/);
        assert.doesNotMatch(html, /automatic FFmpeg conversion|Windows 6\.4\.38\.2 is private|Previous installer/);
        assert.match(html, /Crisp gold reversed-R/);
        assert.match(html, /installed Windows codecs/);
        for (const [, href] of html.matchAll(/href="([^"]+)"/g)) {
            if (/^(?:https?:|#|mailto:)/.test(href)) continue;
            assert.ok(fs.existsSync(path.join(root, href.split('?')[0])), `${file}: ${href}`);
        }
        const launch = html.match(/<a id="ai-warp-open-full"[^>]*>/)[0];
        assert.doesNotMatch(launch, /target="_blank"/);
        assert.match(launch, /\?v=6\.4\.38\.2/);
    }
});

test('showcase controllers resolve runtime and documentation links from the detected folder', () => {
    for (const file of pages) {
        const html = showcase(fs.readFileSync(path.join(root, file), 'utf8'));
        const elements = new Map([...html.matchAll(/id="([^"]+)"/g)].map(([, id]) => [
            id, { textContent: '', addEventListener() {}, scrollIntoView() {} }
        ]));
        const context = vm.createContext({
            document: { getElementById: id => elements.get(id) || null },
            window: { location: { protocol: 'file:' } }
        });
        const script = [...html.matchAll(/<script\b[^>]*>([\s\S]*?)<\/script>/g)].at(-1)[1];
        new vm.Script(script, { filename: file }).runInContext(context);
        assert.equal(elements.get('ai-warp-open-full').href,
            'ai_warp_engine/index.html?v=6.4.38.2');
        for (const [id, element] of elements) {
            if (!element.href || /^https?:/.test(element.href)) continue;
            assert.ok(fs.existsSync(path.join(root, element.href.split('?')[0])), `${file}: ${id}`);
        }
    }
});

test('upload runtime uses existing locally bundled scripts, styles and media', () => {
    const app = path.join(root, 'ai_warp_engine');
    const html = fs.readFileSync(path.join(app, 'index.html'), 'utf8');
    const sources = [...html.matchAll(/(?:src|href)="([^"]+)"/g)]
        .map(([, src]) => src).filter(src => !/^(?:https?:|#|data:)/.test(src));
    for (const src of sources) {
        assert.ok(fs.existsSync(path.join(app, src.split('?')[0])), src);
    }
    assert.ok(fs.existsSync(path.join(app, 'THREE-LICENSE.txt')));
    assert.equal(fs.readdirSync(path.join(app, 'audio')).length, 10);
});
