/**
 * AI WARP ENGINE - YouTube overlay bridge (v6.4.8)
 *
 * ai_warp_engine.js is the unmodified original. The YouTube iframe is cross-origin and ignores SVG
 * reference filters (one url(#...) also cancels the rest of the filter chain), so this bridge:
 *   1. strips the url(#ytSvgWarp) reference from the shell filter when it holds an iframe, so the
 *      original CSS transform / filter / clip-path Warp Bus works on the real video, and
 *   2. draws a per-mode compositor overlay (backdrop-filter bands, masked rings, blend tints) above it.
 * The original engine code is called first on every frame and is never altered.
 */
(function (root) {
    'use strict';
    const Engine = root.AiWarpEngine;
    if (!Engine || !Engine.prototype.updateYouTubeTransformsAndFilters) return;
    const c01 = (v) => Math.max(0, Math.min(1, v));

    /* YouTube overlay FX (v6.4.6). The YouTube iframe is cross-origin and its pixels can't be read or
       SVG-filtered, so each mode is built from layers the compositor CAN apply over it:
       b = backdrop-filter bands [count, minH, maxH, fxList, shift, jump, roll]
       r = masked backdrop ring/wedge/edge/checker layer [kind, fx, param]
       t = blend-mode tint [gradient, blend, alpha]; g = full-frame backdrop fx [fx, alpha]
       grid/scan/vig = overlay alphas. */
    const G_CM = 'linear-gradient(100deg,#00e5ff,#7a1fa2,#ff2bd6)';
    const G_RAD = (c) => `radial-gradient(circle,${c},rgba(0,0,0,0) 68%)`;
    const YT_OV = {
        liquid:              { b: [12, .05, .14, ['hue', 'sat'], .07], t: [G_CM, 'color', .5] },
        prism:               { b: [14, .02, .08, ['hue', 'inv'], .05], t: ['linear-gradient(90deg,#f00,#0f0,#00f)', 'screen', .32] },
        breathe:             { r: ['rings', 'bright', 70], t: [G_RAD('#ff50a0'), 'overlay', .65] },
        fold:                { b: [8, .08, .22, ['inv', 'sat'], .12], t: ['linear-gradient(0deg,#ff7a00,#6a00ff)', 'soft-light', .6] },
        aurora:              { b: [5, .1, .25, ['hue'], .04], t: ['linear-gradient(180deg,#00ffa0,#7800ff)', 'screen', .55] },
        vortex:              { r: ['wedges', 'hue', 24], t: [G_RAD('#fff'), 'overlay', .5] },
        cybergrid:           { b: [8, .02, .07, ['sat', 'inv'], .05, 1], grid: .6 },
        hyperdrive:          { r: ['rings', 'bright', 50], t: [G_RAD('#fff'), 'color-dodge', .6] },
        kaleidoscope:        { r: ['wedges', 'inv', 18], t: ['conic-gradient(#f0f,#0ff,#ff0,#f0f)', 'color', .5] },
        quantum:             { b: [14, .01, .05, ['inv', 'dark', 'hue'], .1, 1] },
        ripple:              { r: ['rings', 'inv', 90], t: [G_RAD('#00e5ff'), 'screen', .3] },
        analog_crt:          { b: [3, .03, .08, ['bright'], .02], scan: .6, vig: .55 },
        newblue_roll:        { b: [2, .12, .25, ['dark', 'bright'], 0, 0, 1] },
        newblue_active_cam:  { b: [6, .03, .1, ['sat', 'hue'], .04], r: ['edge', 'sat', 0] },
        newblue_shear:       { b: [12, .06, .1, ['hue'], .16] },
        newblue_wiggle:      { b: [14, .02, .06, ['sat'], .03] },
        newblue_zoom_blur:   { r: ['edge', 'blur', 0], t: [G_RAD('#fff'), 'overlay', .35] },
        newblue_video_tile:  { r: ['checker', 'inv', 20] },
        newblue_twirl:       { r: ['wedges', 'hue', 40], t: [G_RAD('#fff'), 'overlay', .3] },
        newblue_light_ring:  { r: ['rings', 'bright', 120], t: [G_RAD('#ffffc8'), 'screen', .65] },
        newblue_earthquake:  { b: [14, .02, .09, ['inv', 'dark'], .12, 1] },
        newblue_water_drop:  { r: ['rings', 'blur', 110], t: [G_RAD('#00c8ff'), 'color', .3] },
        newblue_rgb_shift:   { b: [8, .03, .07, ['hue'], .06, 1], t: ['linear-gradient(90deg,#f00,rgba(0,0,0,0) 40%,rgba(0,0,0,0) 60%,#0ff)', 'screen', .65] },
        newblue_quad_mirror: { r: ['checker', 'hue', 50], t: ['conic-gradient(#f0f,#0ff,#f0f)', 'soft-light', .5] },
        newblue_fish_eye:    { r: ['edge', 'blur', 0], vig: .65 },
        newblue_pinch:       { r: ['edge', 'sat', 0], vig: .45 },
        newblue_solarize:    { g: ['solar', .85], b: [4, .15, .3, ['inv'], 0] },
        newblue_cathode_beam:{ b: [1, .05, .1, ['bright'], 0, 0, 1], scan: .7, vig: .45 },
        newblue_neon_edges:  { g: ['neon', .9], grid: .5 }
    };
    const ovFx = (k, h) => ({
        inv: `invert(1) hue-rotate(${h | 0}deg) saturate(1.8)`,
        hue: `hue-rotate(${h | 0}deg) saturate(3.2) contrast(1.3)`,
        sat: 'saturate(4.5) contrast(1.8) brightness(1.15)',
        bright: 'brightness(2.6) contrast(1.5) saturate(2)',
        blur: 'blur(8px) saturate(1.8) brightness(1.15)',
        dark: 'brightness(0.2) contrast(1.8)',
        neon: 'contrast(2.4) saturate(3.4) brightness(1.15)',
        solar: `invert(1) contrast(1.5) hue-rotate(${h | 0}deg)`
    })[k] || 'none';

    const Overlay = {
    updateYtOverlay(s, t, mode, bass, mid, treble) {
        if (!this._ytOv) {
            const stage = document.getElementById('canvas-stage');
            const shell = document.getElementById('ytWarpShell');
            if (!stage || !shell) return;
            const host = document.createElement('div');
            host.id = 'ytFxOverlay';
            host.style.cssText = 'position:absolute;inset:0;z-index:1;pointer-events:none;overflow:hidden;display:none';
            shell.insertAdjacentElement('afterend', host);
            const mk = (css) => { const d = document.createElement('div'); d.style.cssText = 'position:absolute;display:none;' + css; host.appendChild(d); return d; };
            const bands = [];
            for (let i = 0; i < 14; i++) bands.push(mk('left:-6%;width:112%;'));
            this._ytOv = {
                host, bands,
                ring: mk('inset:0;-webkit-mask-repeat:no-repeat;mask-repeat:no-repeat;-webkit-mask-position:center;mask-position:center;'),
                glob: mk('inset:0;'),
                tint: mk('inset:0;'),
                grid: mk('inset:0;mix-blend-mode:screen;background:repeating-linear-gradient(0deg,rgba(0,255,255,.55) 0 1px,transparent 1px 36px),repeating-linear-gradient(90deg,rgba(255,43,214,.55) 0 1px,transparent 1px 36px);'),
                scan: mk('inset:0;background:repeating-linear-gradient(0deg,rgba(0,0,0,.55) 0 1px,transparent 1px 3px);'),
                vig: mk('inset:0;background:radial-gradient(ellipse at center,rgba(0,0,0,0) 45%,rgba(0,0,0,.9) 100%);')
            };
        }
        const ov = this._ytOv;
        const cfg = YT_OV[mode];
        if (!cfg || s <= 0.005) { ov.host.style.display = 'none'; return; }
        ov.host.style.display = 'block';
        const c01 = (v) => Math.max(0, Math.min(1, v));
        const hash = (x) => { const v = Math.sin(x * 12.9898) * 43758.5453; return v - Math.floor(v); };
        const show = (el, on) => { const d = on ? 'block' : 'none'; if (el.style.display !== d) el.style.display = d; };
        const bf = (el, v) => { el.style.backdropFilter = v; el.style.webkitBackdropFilter = v; };
        const msk = (el, v) => { el.style.maskImage = v; el.style.webkitMaskImage = v; };
        const pump = 1 + bass * 0.7;
        const W = ov.host.clientWidth || 1280;

        const [n, hMin, hMax, fxList, shift, jump, roll] = cfg.b || [0];
        for (let i = 0; i < ov.bands.length; i++) {
            const el = ov.bands[i];
            if (i >= n) { show(el, false); continue; }
            const js = jump ? Math.floor(t * 8) : 0;
            const h1 = hash(i * 7.1 + js), h2 = hash(i * 3.3 + js * 1.7);
            const on = jump ? h1 > (1 - (0.3 + 0.65 * s)) : true;
            show(el, on);
            if (!on) continue;
            let y = roll ? ((t * 0.35 + i * 0.5) % 1)
                : jump ? h2
                : (((i * 0.618 + 0.1 * Math.sin(t * 0.9 + i) + t * 0.05 * (1 + (i % 3))) % 1) + 1) % 1;
            const hh = hMin + (hMax - hMin) * h1;
            el.style.top = (y * 100).toFixed(2) + '%';
            el.style.height = (hh * 100).toFixed(2) + '%';
            el.style.transform = `translateX(${(Math.sin(t * 3 + i * 1.9) * shift * W * s * pump).toFixed(1)}px)`;
            bf(el, ovFx(fxList[i % fxList.length], i * 47 + t * 70));
            el.style.opacity = c01(s * (0.6 + 0.5 * bass) * (0.65 + 0.35 * h2)).toFixed(3);
        }

        if (cfg.r) {
            const [kind, fx, p] = cfg.r;
            const el = ov.ring;
            show(el, true);
            bf(el, ovFx(fx, t * 60));
            el.style.opacity = c01(s * 0.9 * pump).toFixed(3);
            let mask, size = '100% 100%';
            if (kind === 'rings') {
                mask = `repeating-radial-gradient(circle at 50% 50%,#000 0 ${p * 0.45}px,transparent ${p * 0.45}px ${p}px)`;
                const z = 130 + 90 * (0.5 + 0.5 * Math.sin(t * 2.2));
                size = `${z}% ${z}%`;
            } else if (kind === 'wedges') {
                const a = 360 / p;
                mask = `repeating-conic-gradient(from ${((t * 40) % 360).toFixed(1)}deg at 50% 50%,#000 0 ${a / 2}deg,transparent ${a / 2}deg ${a}deg)`;
            } else if (kind === 'edge') {
                const inner = 18 + 22 * (0.5 + 0.5 * Math.sin(t * 2));
                mask = `radial-gradient(circle at 50% 50%,transparent 0 ${inner}%,#000 ${inner + 38}%)`;
            } else {
                mask = 'repeating-conic-gradient(#000 0 25%,transparent 0 50%)';
                size = `${p}% ${p}%`;
            }
            msk(el, mask);
            el.style.webkitMaskSize = size; el.style.maskSize = size;
        } else show(ov.ring, false);

        if (cfg.g) {
            const el = ov.glob;
            show(el, true);
            bf(el, ovFx(cfg.g[0], t * 50));
            const flick = cfg.g[0] === 'solar' ? (0.55 + 0.45 * Math.sin(t * 2.6)) : 1;
            el.style.opacity = c01(cfg.g[1] * s * flick).toFixed(3);
        } else show(ov.glob, false);

        if (cfg.t) {
            const el = ov.tint;
            show(el, true);
            el.style.background = cfg.t[0];
            el.style.mixBlendMode = cfg.t[1];
            el.style.opacity = c01(cfg.t[2] * s * (0.8 + 0.4 * bass)).toFixed(3);
        } else show(ov.tint, false);

        for (const k of ['grid', 'scan', 'vig']) {
            const el = ov[k];
            if (cfg[k]) { show(el, true); el.style.opacity = c01(cfg[k] * s).toFixed(3); } else show(el, false);
        }
    }
    };

    const origUpdate = Engine.prototype.updateYouTubeTransformsAndFilters;
    Engine.prototype.updateYouTubeTransformsAndFilters = function (now, dt) {
        origUpdate.call(this, now, dt);
        const shell = document.getElementById('ytWarpShell');
        if (!shell) return;
        if (shell.style.display === 'none') {
            if (this._ytOv) this._ytOv.host.style.display = 'none';
            return;
        }
        if (shell.querySelector('iframe') && shell.style.filter.indexOf('url(') !== -1) {
            shell.style.filter = shell.style.filter.replace(/url\([^)]*\)\s*/g, '').trim() || 'none';
        }
        const raw = (this.aiWarpActive && this.aiWarpStrength > 0.005) ? (this.aiWarpStrength * this.masterOpacity) : 0;
        const s = raw > 0.005 ? Math.min(1, raw * 1.35 + 0.15) : 0;
        const au = this.audioDrive || {};
        Overlay.updateYtOverlay.call(this, s, now * 0.001 * this.glitchSpeed, this.aiWarpMode, au.bass || 0, au.mid || 0, au.treble || 0);
    };

    const origSource = Engine.prototype.setSourceMode;
    Engine.prototype.setSourceMode = function (mode, element) {
        const r = origSource.apply(this, arguments);
        if (mode !== 'youtube' && this._ytOv) this._ytOv.host.style.display = 'none';
        return r;
    };
})(typeof window !== 'undefined' ? window : this);
