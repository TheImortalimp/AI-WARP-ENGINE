# ⚡ AI WARP ENGINE — R3 Canvas Edition (v6.4 · Display Capture + YouTube Warp Bus + SVG Lens)

> **Public-domain visual synthesizer, audio-reactive media playground, and live Canvas glitch suite.**  
> Created through collaborative work between [imortalimp.nl](https://imortalimp.nl) (River Lyle Reuveni), [Arena.ai](https://arena.ai), and artificial intelligence.

---

## Release highlights

1. **34 independently controllable glitch voices** — the original 17 voices remain intact, with 17 separate Glitch Revision 3 voices identified by the exact `GC#(3)` suffix.
2. **29 AI Warp modes** — liquid, prism, breathe, fold, aurora, vortex, cybergrid, hyperdrive, kaleidoscope, quantum, ripple, analog CRT, plus the 17 NewBlue-inspired modes.
2a. **🖥 Browser Display Capture (v6.4)** — `getDisplayMedia` lets you share a browser tab, application window, or full screen; shared frames feed `#localVideo` and the full Canvas pipeline. Optional shared audio and the independently enabled microphone both feed `masterAnalyser` for analysis only (no duplicated playback). YouTube's existing iframe/Canvas Warp Bus and synthetic audio-reactive modes remain available; screen sharing temporarily takes visual focus and restores the YouTube Canvas feed when stopped. Use `N` to toggle display capture, `M` to toggle the microphone, `Y` to load the featured YouTube playlist into the engine, and `E` to cycle the Audio Reactive Ear (`AUTO` / `AUDIO` / `CANVAS`).
2b. **YouTube Warp Bus** — the live YouTube feed (cross-origin iframe, unreadable by Canvas) is drawn directly onto the effects pipeline: every warp mode drives 3D transforms, clip-path tears and seamless spins on `#ytWarpShell`, and all 34 voices (core + `GC#(3)`) contribute to a cumulative CSS filter chain (hue cycles, invert, strobe, flash, neon chroma-split echoes, VHS jitter, data-band tears) on the real video picture, while self-drawing voices keep overlaying the stacked glitch canvas. Playback body classes (`playback-on`, `local-video`, `youtube-feed`) are now actually toggled, activating the R3 factories' live-feed branches. **v6.3.1 adds the SVG Warp Lens**: the shell is additionally rasterized through an inline `feTurbulence → feDisplacementMap` graph (`#ytSvgWarp`) for true per-pixel melting/tearing/liquefaction of the live video — compositor-level and display-only (pixels are never read back into JS), per-mode noise character, voice-boosted amplitude, toggled with `aiWarp.setSvgWarpActive()`.
3. **Ghost Protocol R3/v11** — DIA, SPECTRUM, CRT, PLASMA, and LED modes with channel splits, Old-Soul echo, scan/warp behavior, CRT/LED texture, and color/DIA confluence.
4. **V-Sync R3/v11** — Scanphasing, Color Harmony, and Sharp modes with scan bands, tracking tears, roll, phosphor texture, per-mode levels, V-Sync locking, and retained OFF rails.
5. **R3 automation confluence** — Auto Glitch, Continual Randomization, Timed Automation, Automated Masters, Soft-Eye/Crescendo, and visual/audio-ear signals can drive Ghost/V-Sync targets and synchronized light bursts.
5a. **Mode-aware FX blending** — each AI Warp mode deterministically tunes Ghost/V-Sync color, split, scan, warp, and automation pacing; the original master levels and protected voice intent remain respected.
5b. **DROP THE MILK? subtle blends (v6.4.19)** — softer Milkdrop feedback, waves and blooms, with lower preset opacity while retaining analyser reactivity, all 80 shuffled presets, AI Warp/voice blending and the persistent opacity lock.
5c. **YT LENS playlist playback (v6.4.19)** — playlist-only suggestions route through the standalone lens and its audio-aware capture; starting another source closes the lens and stops playback.
6. **Canvas/WebGL effects core** — all voice rendering stays on Canvas/WebGL. The single sanctioned SVG usage (v6.3.1) is the compositor-level Warp Lens (`feTurbulence → feDisplacementMap`) on the YouTube shell only — still no JS pixel readback, no SVG DOM mutation beyond the two animated displacement attributes, and no SVG filters in the canvas pipeline.
7. **Media studio** — local image/video/audio support, YouTube controls and knowledgebase, microphone monitoring, and procedural synthetic audio.
8. **Performance controls** — held Ghost/V-Sync rails, direct null controls, locks, 80 randomized machine presets, user presets, live mixer levels, and keyboard shortcuts.
9. **Protected voice intent** — user-disabled or locked voices are respected by background automation.
10. **Public domain release** — distributed under CC0 1.0 / The Unlicense.

---

## Launch

Open any of these identical application aliases in a modern browser:

- `index.html`
- `ai_warp_engine.html`
- `media_page.html`

The application loads local scripts in this dependency order:

1. `glitch_engine.js`
2. `r3_gc3_factories.js`
3. `r3_ghost_vsync.js`
4. `ai_warp_engine.js`
5. `media_page.js`

A local HTTP server is recommended instead of `file://` when using YouTube, media loading, or browser APIs. For example:

```bash
python3 -m http.server 8000
```

Then open `http://localhost:8000/media_page.html`.

---

## Core controls

### Glitch voice mixer

The mixer contains **34 reachable rails**:

- **17 Core voices:** Neon Grad, Scanlines, Data Bands, Flash, HSL Rainbow, ASCII, Font Burn, Strobe, Gemini Bomb, Tech Artist, VHS Glitch, Data Bomb, Ink Glyphs, Matrix ASCII, Matrix Chord, B&W Invert, and Hue/Color Cycle.
- **17 R3 voices:** the same names with the exact suffix ` GC#(3)`.

The GC#(3) factory block is isolated from the original voice bank so it cannot overwrite the existing 17 rails. Dynamic presets deliberately cover every R3 factory, while legacy 17-voice presets gain restrained R3 companion levels at apply time unless an explicit `GC#(3)` value is already saved. The mixer viewport measures its surrounding rails so its final entries remain reachable above the automation dock.

### Ghost Protocol R3/v11

Use the **GHOST** button to cycle:

`OFF → DIA → SPECTRUM → CRT → PLASMA → LED → OFF`

- The Ghost level slider is a manual rail and temporarily protects the selected level from automation.
- Cycling to OFF bypasses the effect while retaining its level for fast re-entry.
- The dedicated Ghost Null control clears the level and mode completely.

### V-Sync R3/v11

Use the **V-SYNC** button to cycle:

`OFF → SCANPHASING → COLOR HARMONY → SHARP → OFF`

Internally these correspond to `ON`, `GHOST_OFF`, and `FULL` R3 methods.

- Each active V-Sync method retains its own level/target script.
- OFF is a bypass and preserves the most recent live V-Sync rail.
- The lock prevents automated or manual mode changes until unlocked.
- The dedicated V-Sync Null control clears the retained rail.

### Automation and R3 confluence

| Control | R3 system behavior |
|---|---|
| **Auto Glitch** | Schedules paired core + `GC#(3)` voice sparks and synchronized Ghost/V-Sync light bursts. |
| **Continual Rand** | Rotates a tracked core + `GC#(3)` factory pair, gently modulates unlocked active rails, and drifts active Ghost targets without retiring manual selections. |
| **Timed Auto** | Selects cadence-synchronized core/R3 factory pairs and adds color/DIA burst events. |
| **Auto Masters** | Retargets unlocked Ghost/V-Sync rails and a complementary core/R3 accent pair with the master board. |
| **Soft-Eye** | Drives the R3 crescendo state machine, visual/audio ear, and burst gate. |
| **Crescendo** | Triggers a high-impact Ghost/V-Sync burst plus temporary Flash/Strobe flares in both voice banks. |
| **DROP THE MILK?** | Layers audio-reactive Canvas feedback and polar waveforms; cycles 80 shuffled presets whose opacity can be automated or locked by the user. |
| **Presets** | Store and restore Ghost mode/level and V-Sync mode-specific rail levels. |

---

## Keyboard shortcuts

| Shortcut | Action |
|---|---|
| `Space` | Play/pause the active media source or synthetic audio |
| `W` | Cycle AI Warp mode |
| `G` | Cycle Ghost Protocol mode |
| `V` | Cycle V-Sync method |
| `A` | Toggle Auto Glitch |
| `R` | Randomize the effects board |
| `T` | Cycle Timed Automation cadence |
| `C` | Trigger Crescendo |
| `D` | Toggle DROP THE MILK? feedback FX |
| `Z` | EMP Purge / zero all voices and R3 rails |
| `M` | Toggle microphone monitor |
| `H` | Toggle fullscreen HUD state |
| `S` | Export a frozen PNG frame |
| `1`–`9` | Toggle the corresponding first nine mixer rails |
| `?` | Open/close the shortcuts dialog |

---

## Files

| File | Purpose |
|---|---|
| `index.html` | Primary application alias |
| `ai_warp_engine.html` | Application alias |
| `media_page.html` | Application alias |
| `ai_warp_engine.js` | Main Canvas/WebGL visual engine, 34-voice mixer, warp modes, and automation bridge |
| `r3_gc3_factories.js` | Isolated Glitch Revision 3 factory bank for the 17 `GC#(3)` voices |
| `r3_ghost_vsync.js` | Canvas-only Ghost Protocol R3/v11 and V-Sync R3/v11 adapter |
| `glitch_engine.js` | Shared profiles, preset helpers, Soft-Eye/Crescendo, burst gate, and ear utilities |
| `media_page.js` | UI, media, audio, mixer, controls, presets, and hotkeys |
| `media_page.css` | Responsive cyberpunk HUD styling |
| `media_page.json` | Track, profile, and machine-preset data |
| `REVISION_NOTES.md` | Concise revision and verification notes |

---

## Implementation note: Canvas-only R3 system

The GC#(3) voice factories retain their copied Revision 3 factory block. Ghost Protocol and V-Sync were integrated as a Canvas-only R3 adapter because the source page’s former SVG-filter path is intentionally excluded from this release. The adapter uses ordinary Canvas compositing, Canvas 2D filter primitives, snapshots, gradients, scan slices, and offscreen canvases — never an SVG filter URL or SVG filter element.

---

## License and attribution

This software is dedicated to the **Public Domain** under the [CC0 1.0 Universal Public Domain Dedication](https://creativecommons.org/publicdomain/zero/1.0/) and [The Unlicense](https://unlicense.org/).

You may copy, modify, publish, use, compile, sell, or distribute it for any purpose, with or without attribution.

- **Project genesis & architecture:** River Lyle Reuveni / [imortalimp.nl](https://imortalimp.nl)
- **Agent engineering & synthesis:** [Arena.ai](https://arena.ai) Agent Mode
- **Collaborative foundation:** Generative AI and the open-source community
