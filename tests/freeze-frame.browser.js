'use strict';

// Host-run proof for the Full FX Freeze Frame. In the running native app: await runFreezeFrameChecks().
// Every node outside the capture layers is painted magenta (a "sentinel"). The freeze frame must contain none of
// it, while an unmasked control capture must show it, so the test can never pass by being blind.
async function runFreezeFrameChecks() {
    const engine = window.aiWarpEngine;
    if (!engine) throw new Error('Engine not initialized.');
    if (!window.chrome || !window.chrome.webview) throw new Error('The native WebView2 bridge is unavailable.');
    cancelAnimationFrame(engine.animFrameId);
    const canvas = engine.canvas;
    const context = canvas.getContext('2d');
    context.clearRect(0, 0, canvas.width, canvas.height);
    context.fillStyle = '#609080';
    context.fillRect(0, 0, canvas.width, canvas.height);
    canvas.style.filter = 'none';
    const page = document.documentElement;
    page.style.filter = 'none';
    const check = (condition, message) => { if (!condition) throw new Error(message); };
    const sleep = ms => new Promise(resolve => setTimeout(resolve, ms));
    const frames = async count => {
        for (let i = 0; i < count; i++) {
            await new Promise(resolve => {
                const timer = setTimeout(resolve, 100);
                requestAnimationFrame(() => { clearTimeout(timer); resolve(); });
            });
        }
    };
    async function decode(url) {
        const image = new Image();
        image.src = url;
        await image.decode();
        const surface = document.createElement('canvas');
        surface.width = image.width;
        surface.height = image.height;
        const ctx = surface.getContext('2d', { willReadFrequently: true });
        ctx.drawImage(image, 0, 0);
        const data = ctx.getImageData(0, 0, surface.width, surface.height).data;
        return {
            width: image.width, height: image.height, data,
            at: (x, y) => { const i = (y * image.width + x) * 4; return [data[i], data[i + 1], data[i + 2], data[i + 3]]; },
            mean: (cx, cy, half) => {
                let r = 0, g = 0, b = 0, n = 0;
                for (let y = Math.max(0, cy - half); y <= Math.min(image.height - 1, cy + half); y++) {
                    for (let x = Math.max(0, cx - half); x <= Math.min(image.width - 1, cx + half); x++) {
                        const i = (y * image.width + x) * 4;
                        r += data[i]; g += data[i + 1]; b += data[i + 2]; n++;
                    }
                }
                return [Math.round(r / n), Math.round(g / n), Math.round(b / n)];
            },
            countIn: (rect, test) => {
                const scale = image.width / document.documentElement.clientWidth;
                const x0 = Math.max(0, Math.floor(rect.left * scale)), x1 = Math.min(image.width - 1, Math.ceil(rect.right * scale));
                const y0 = Math.max(0, Math.floor(rect.top * scale)), y1 = Math.min(image.height - 1, Math.ceil(rect.bottom * scale));
                let total = 0;
                for (let y = y0; y <= y1; y++) {
                    for (let x = x0; x <= x1; x++) {
                        const i = (y * image.width + x) * 4;
                        if (test(data[i], data[i + 1], data[i + 2])) total++;
                    }
                }
                return total;
            },
            count: test => {
                let total = 0;
                for (let i = 0; i < data.length; i += 4) if (test(data[i], data[i + 1], data[i + 2])) total++;
                return total;
            }
        };
    }

    // ---- HUD enumeration, deliberately independent of the mask selector under test ----
    const isMagenta = (r, g, b) => r > 170 && b > 170 && g < 100;
    const NOT_RENDERED = 'script, style, link, meta, template, noscript';
    const CAPTURE_LAYERS = 'svg.svg-warp-defs, .crt-overlay, .cyber-vignette';
    const layer = document.getElementById('fullscreen-fx-layer');
    const describe = node => {
        const cls = typeof node.className === 'string' && node.className.trim() ? '.' + node.className.trim().split(/\s+/)[0] : '';
        return node.tagName.toLowerCase() + (node.id ? '#' + node.id : '') + cls;
    };
    const hudRoots = () => [...document.body.children, ...layer.children].filter(node =>
        node !== layer && node.id !== 'canvas-stage' && !node.matches(NOT_RENDERED) && !node.matches(CAPTURE_LAYERS));
    const subtree = root => [root, ...root.querySelectorAll('*')].filter(node => !node.matches(NOT_RENDERED));
    const hudNodes = () => hudRoots().flatMap(subtree);

    // The capture layers may only contain media/effect nodes: a HUD node hidden in one would be captured.
    const stage = [...document.getElementById('canvas-stage').children].map(node => node.id);
    check(JSON.stringify(stage) === JSON.stringify(['ytWarpShell', 'glitchCanvas', 'localVideo', 'localAudio', 'localArtworkImg']),
        `Unexpected node inside the captured stage: ${stage.join(', ')}`);
    const layerKids = [...layer.children].map(node => node.id || node.className.split(' ')[0]);
    check(['crt-overlay', 'cyber-vignette'].every(name => layerKids.includes(name)), 'The CRT/vignette layers are missing.');

    function paint(nodes) {
        const saved = nodes.map(node => node.getAttribute('style'));
        for (const node of nodes) {
            for (const [name, value] of [['background', '#f0f'], ['outline', '4px solid #f0f'], ['color', '#f0f'],
                ['fill', '#f0f'], ['stroke', '#f0f'], ['opacity', '1']]) node.style.setProperty(name, value, 'important');
        }
        return () => nodes.forEach((node, i) => saved[i] === null ? node.removeAttribute('style') : node.setAttribute('style', saved[i]));
    }
    // Reproduces the original leak: "transition: all" keeps a node painted while its visibility animates away.
    function addTransitionStress() {
        const style = document.createElement('style');
        const roots = 'body > :not(#canvas-stage):not(#fullscreen-fx-layer):not(.svg-warp-defs)';
        style.textContent = `${roots}, ${roots} *, #fullscreen-fx-layer > :not(.crt-overlay):not(.cyber-vignette)` +
            ', #fullscreen-fx-layer > :not(.crt-overlay):not(.cyber-vignette) * { transition: all 1.5s linear; }';
        document.head.appendChild(style);
        return () => style.remove();
    }
    function openOverlays(only) {
        const opened = [];
        for (const [selector, name] of [['#dropOverlay', 'active'], ['#pole-r-drawer', 'open'], ['#ytKbModal', 'open'],
            ['#shortcutsModal', 'open'], ['#eqDrawer', 'open']]) {
            const node = document.querySelector(selector);
            if (!node || (only && node !== only) || node.classList.contains(name)) continue;
            node.classList.add(name);
            opened.push([node, name]);
        }
        return () => opened.forEach(([node, name]) => node.classList.remove(name));
    }
    // Brings one HUD root into a state where nothing covers it, so its sentinel really reaches the control screenshot.
    function expose(root) {
        const closeOverlay = openOverlays(root);
        const savedStyle = root.getAttribute('style');
        // A child of the z-index 2 FX layer can only outrank the HUD bars if the whole layer does.
        const inLayer = root.parentElement === layer;
        const layerStyle = layer.getAttribute('style');
        if (inLayer) layer.style.setProperty('z-index', '2147483000', 'important');
        const banner = root.id === 'hudNotificationBanner';
        if (banner) root.classList.add('show');
        if (root.matches('.vcr-osd-badge, #hudNotificationBanner')) {
            const placement = banner ? [['top', '60%']] : [['top', '45%'], ['left', '40%'], ['right', 'auto']];
            for (const [name, value] of [['position', 'fixed'], ['z-index', '2147483000'], ...placement]) {
                root.style.setProperty(name, value, 'important');
            }
        }
        return () => {
            closeOverlay();
            if (banner) root.classList.remove('show');
            if (savedStyle === null) root.removeAttribute('style');
            else root.setAttribute('style', savedStyle);
            if (inLayer) {
                if (layerStyle === null) layer.removeAttribute('style');
                else layer.setAttribute('style', layerStyle);
            }
        };
    }
    let rawSequence = 0;
    // A native screenshot WITHOUT the freeze mask: the control that proves the sentinel is visible.
    function unmaskedNativeCapture() {
        const bridge = window.chrome.webview;
        const id = String(Date.now()) + String(++rawSequence).padStart(3, '0');
        return new Promise((resolve, reject) => {
            const timer = setTimeout(() => { bridge.removeEventListener('message', onMessage); reject(new Error('Unmasked control capture timed out.')); }, 10000);
            function onMessage(event) {
                if (typeof event.data !== 'string' || !event.data.startsWith('freeze:result|')) return;
                const result = JSON.parse(event.data.substring(14));
                if (result.id !== id) return;
                clearTimeout(timer);
                bridge.removeEventListener('message', onMessage);
                if (result.error) reject(new Error(result.error));
                else resolve('data:image/png;base64,' + result.png);
            }
            bridge.addEventListener('message', onMessage);
            bridge.postMessage('freeze:capture|' + id);
        });
    }

    const capture = new FreezeFrameCapture(window.chrome.webview);
    async function sentinelRun(nodes, { minControl, stress, browser }) {
        const restore = paint(nodes);
        let removeStress = () => {};
        try {
            await sleep(500); // let the app's own transitions finish before the sentinel is observed
            if (stress) removeStress = addTransitionStress();
            await frames(3);
            const control = (await decode(await unmaskedNativeCapture())).count(isMagenta);
            check(control >= minControl, `Unmasked control saw ${control} sentinel pixels (need ${minControl}); the HUD test would be blind.`);
            page.classList.add('freeze-frame-capture');
            await frames(2);
            const stillVisible = nodes.filter(node => getComputedStyle(node).visibility !== 'hidden').map(describe);
            page.classList.remove('freeze-frame-capture');
            const masked = await decode(await capture.capture(canvas, 'video'));
            const result = { control, leaked: masked.count(isMagenta), stillVisible, size: [masked.width, masked.height] };
            if (browser) result.browserLeaked = (await decode(await new FreezeFrameCapture(null).capture(canvas, 'video'))).count(isMagenta);
            return result;
        } finally {
            removeStress();
            restore();
        }
    }
    // Leaks are collected so a failing build reports every leaking state at once; a blind control still throws at once.
    const failures = [];
    async function proveState(label, options) {
        const nodes = hudNodes();
        check(nodes.length > 50, `${label}: only ${nodes.length} HUD nodes found; the HUD scan is broken.`);
        const result = await sentinelRun(nodes, options);
        const problems = [];
        if (result.stillVisible.length) {
            problems.push(`${result.stillVisible.length} HUD nodes stay visible under the freeze mask (${result.stillVisible.slice(0, 3).join(', ')})`);
        }
        if (result.leaked) problems.push(`${result.leaked} HUD pixels leaked into the native freeze frame`);
        if (result.browserLeaked) problems.push(`${result.browserLeaked} HUD pixels leaked into the browser freeze frame`);
        if (problems.length) failures.push(`${label}: ${problems.join('; ')}`);
        return { hudNodes: nodes.length, controlPixels: result.control, leakedPixels: result.leaked, visibleNodes: result.stillVisible.length };
    }

    const topBar = document.getElementById('glitch-top-bar');
    const topBarStyle = topBar.getAttribute('style');
    const hud = {};

    // 1. Normal HUD.
    hud.normal = await proveState('normal HUD', { minControl: 3000, stress: false, browser: true });

    // 1b. HUD notifications must be visible where users actually see them: at their natural position, above the HUD bars.
    const notice = document.getElementById('hudNotificationBanner');
    const restoreNotice = paint([notice]);
    notice.classList.add('show');
    let noticeSeen = 0;
    let noticeArea = 0;
    try {
        await sleep(800);
        const rect = notice.getBoundingClientRect();
        noticeArea = Math.round(rect.width * rect.height);
        noticeSeen = (await decode(await unmaskedNativeCapture())).countIn(rect, isMagenta);
    } finally {
        notice.classList.remove('show');
        restoreNotice();
    }
    if (notice.parentElement !== document.body || noticeSeen < noticeArea * 0.5) {
        failures.push(`HUD notification banner is covered by the HUD at its natural position (${noticeSeen} of ${noticeArea} px visible, parent ${notice.parentElement.id || notice.parentElement.tagName})`);
    }

    // 2. Worst case: every overlay open, every HUD node animating with "transition: all".
    const closeOverlays = openOverlays();
    try {
        await sleep(500);
        hud.everythingOpen = await proveState('all overlays open + slow transitions', { minControl: 3000, stress: true });
    } finally {
        closeOverlays();
    }
    await sleep(500);

    // Per-root attribution: each HUD root is exposed on its own, must be visibly painted in the control capture
    // (otherwise its absence proves nothing) and must then be absent from the freeze frame.
    const roots = {};
    for (const root of hudRoots()) {
        const unexpose = expose(root);
        try {
            await sleep(500);
            const result = await sentinelRun(subtree(root), { minControl: 0, stress: true });
            roots[describe(root)] = { control: result.control, leaked: result.leaked };
            const problems = [];
            if (result.leaked) problems.push(`${result.leaked} pixels leaked`);
            if (result.stillVisible.length) problems.push(`${result.stillVisible.length} nodes stay visible (${result.stillVisible.slice(0, 3).join(', ')})`);
            if (problems.length) failures.push(`${describe(root)}: ${problems.join('; ')}`);
        } finally {
            unexpose();
        }
    }
    const unproven = Object.keys(roots).filter(name => roots[name].control < 100);
    check(unproven.length === 0, `HUD roots never visibly painted in the control capture, so their absence proves nothing: ${unproven.join(', ')} ${JSON.stringify(roots)}`);
    await sleep(500);

    // 3. Collapsed HUD: only the "show UI" tab remains, and it forces its own visibility.
    const uiButton = document.getElementById('toggle-ui-btn');
    uiButton.click();
    try {
        check(document.body.classList.contains('ui-collapsed'), 'The UI toggle did not collapse the HUD.');
        await sleep(700);
        hud.collapsed = await proveState('collapsed HUD + slow transitions', { minControl: 150, stress: true });
    } finally {
        if (document.body.classList.contains('ui-collapsed')) uiButton.click();
        await sleep(700);
    }
    check(!document.body.classList.contains('ui-collapsed'), 'The HUD was not restored after the collapsed test.');
    check(failures.length === 0, `HUD leaks into the freeze frame: ${failures.join(' | ')}`);

    // ---- Everything except the HUD must be captured ----
    const native = await decode(await capture.capture(canvas, 'video'));
    check(native.width > 0 && native.height > 0, 'Empty native capture.');
    check(!page.classList.contains('freeze-frame-capture'), 'HUD mask not restored.');
    await frames(2);
    check(getComputedStyle(topBar).visibility === 'visible', 'The HUD stayed hidden after the capture.');
    // An empty style attribute declares nothing, so "no attribute" and style="" are the same inline state.
    check((topBar.getAttribute('style') || '') === (topBarStyle || ''),
        `The test or capture changed the HUD inline style: before=${JSON.stringify(topBarStyle)} after=${JSON.stringify(topBar.getAttribute('style'))}`);
    const center = native.at(Math.floor(native.width / 2), Math.floor(native.height / 2));
    const edge = native.at(1, Math.floor(native.height / 2));
    check(center[1] > 90, 'Canvas/media base missing from native capture.');
    check(edge[1] < center[1] * 0.7, 'Fullscreen vignette missing from native capture.');

    const crt = layer.querySelector('.crt-overlay');
    crt.style.setProperty('display', 'none', 'important');
    let withoutCrt;
    try { withoutCrt = await decode(await capture.capture(canvas, 'video')); }
    finally { crt.style.removeProperty('display'); }
    check(withoutCrt.width === native.width && withoutCrt.height === native.height, 'Capture size changed between shots.');
    let scanlinePixels = 0;
    for (let i = 0; i < native.data.length; i += 4) if (Math.abs(native.data[i + 1] - withoutCrt.data[i + 1]) > 6) scanlinePixels++;
    check(scanlinePixels > native.width * native.height * 0.1, 'CRT scanline overlay missing from native capture.');

    const browser = await decode(await new FreezeFrameCapture(null).capture(canvas, 'video'));
    const browserCenter = browser.at(Math.floor(browser.width / 2), Math.floor(browser.height / 2));
    const browserEdge = browser.at(1, Math.floor(browser.height / 2));
    check(browserEdge[1] < browserCenter[1] * 0.7, 'Browser fullscreen vignette missing.');

    page.style.filter = 'invert(1)';
    const inverted = await decode(await capture.capture(canvas, 'video'));
    const invertedCenter = inverted.at(Math.floor(inverted.width / 2), Math.floor(inverted.height / 2));
    check(Math.abs(invertedCenter[1] - (255 - center[1])) <= 3, 'Page filter missing or applied twice.');
    page.style.filter = 'none';

    // Pixel-producing layers behind the final FX canvas must survive compositor capture.
    context.clearRect(0, 0, canvas.width, canvas.height);
    const shell = document.getElementById('ytWarpShell');
    shell.style.cssText = 'display:block;position:absolute;inset:0;width:100%;height:100%;background:rgb(40,170,80)';
    const media = await decode(await capture.capture(canvas, 'youtube'));
    const mediaCenter = media.mean(Math.floor(media.width / 2), Math.floor(media.height / 2), 8);
    // The CRT scanlines darken alternate rows, so the layer is recognised by hue dominance, not by exact brightness.
    check(mediaCenter[1] > 100 && mediaCenter[1] > mediaCenter[0] * 2.5 && mediaCenter[1] > mediaCenter[2] * 1.6,
        `Underlying LENS/media layer missing (centre mean ${JSON.stringify(mediaCenter)}).`);
    let rejected = false;
    try { await new FreezeFrameCapture(null).capture(canvas, 'youtube'); }
    catch (error) { rejected = /Share the YouTube/.test(error.message); }
    check(rejected, 'Browser cross-origin capture did not fail explicitly.');

    // The mask selector used at runtime must cover every independently enumerated HUD node.
    check(typeof FreezeFrameCapture.HUD_SELECTOR === 'string' && hudNodes().every(node => node.matches(FreezeFrameCapture.HUD_SELECTOR)),
        'The freeze mask selector does not cover every HUD node.');

    return {
        result: 'passed', nativeSize: [native.width, native.height], hud, roots, noticeVisiblePixels: noticeSeen, noticeArea,
        scanlinePixels, nativeCenter: center, nativeEdge: edge, browserCenter, browserEdge, invertedCenter, mediaCenter
    };
}

window.runFreezeFrameChecks = runFreezeFrameChecks;
