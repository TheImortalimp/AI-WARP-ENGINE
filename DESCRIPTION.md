# AI WARP ENGINE — Description

**AI WARP ENGINE (R3 Canvas Edition v6.4)** is a public-domain visual synthesizer, audio-reactive media playground and live Canvas glitch suite, shipped as a standalone Windows app.

- **Author:** River Lyle Reuveni (Imortalimp), [imortalimp.nl](https://imortalimp.nl)
- **Version:** 6.4 (installer release v6.4.8) — **Complete · Ready**
- **Platform:** Windows 10/11 x64 (WebView2 host, .NET bundled)
- **Install location:** `Desktop\AI WARP ENGINE` (per-user, no admin)
- **Licence:** CC0 1.0 / The Unlicense (software); CC BY 4.0 (imortalimp.nl site content)

## What it does

Takes a live source and runs it through a real-time Canvas effects pipeline, driven by an audio-reactive analyser (bass / mid / treble / drive).

**Sources:** built-in generative visuals, local image / video / audio files, YouTube (playlist or URL), a shared tab / window / screen (display capture, `N`), and microphone audio (`M`).

**Effects:**
- 34 independently controllable glitch voices: 17 core (Neon Grad, Scanlines, Data Bands, Flash, HSL Rainbow, ASCII, Font Burn, Strobe, Gemini Bomb, Tech Artist, VHS Glitch, Data Bomb, Ink Glyphs, Matrix ASCII, Matrix Chord, B&W Invert, Hue/Color Cycle) plus 17 Glitch Revision 3 `GC#(3)` counterparts.
- 29 AI Warp modes: 12 algorithmic (liquid, prism, breathe, fold, aurora, vortex, cybergrid, hyperdrive, kaleidoscope, quantum, ripple, analog CRT) and 17 NewBlue-inspired (roll, active cam, shear, wiggle, zoom blur, video tile, twirl, light ring, earthquake, water drop, RGB shift, quad mirror, fish eye, pinch, solarize, cathode beam, neon edges), plus a compositor-level SVG Warp Lens for YouTube.
- Ghost Protocol R3 (DIA, SPECTRUM, CRT, PLASMA, LED) and V-Sync R3 (Scanphasing, Color Harmony, Sharp) overlays, with Soft-Eye and Crescendo.

**Automation:** Auto Glitch, Continual Random, Automated Masters, Timed Automation, Wired mode, Soft-Eye and Crescendo.

**Control:** 80 machine presets plus saved user presets with smooth cross-fades, a live mixer, and keyboard hotkeys (`W` warp, `G` ghost, `V` V-Sync, `A` auto, `R` randomise, `T` timed, `C` crescendo, `Z` zero, `S` freeze, `E` ear, `H` hide UI, `1`–`9` voices, `?` help).

## Safe levels

AI WARP Strength, Master Opacity, Ghost level, V-Sync level and every voice alpha are clamped to 0–1 wherever they are written, and non-numeric values are ignored, so no slider, preset, automation or audio input can push an alpha out of range.

## Verified in the packaged app

- All 34 voices and all 29 warp modes, across every Ghost and V-Sync combination, ran with 0 console errors, 0 out-of-range alpha writes and 0 NaN colours, including with out-of-range audio levels.
- Every automation, Soft-Eye, Crescendo and out-of-range presets were exercised.
- Screen share (`N`) and microphone (`M`) hotkeys confirmed working.

## Release history

- **v6.4.8** — original engine and skewing kept; per-mode compositor FX overlay restored on YouTube playback.
- **v6.4.7** — original AI WARP modes, skewing and YouTube warp path restored; only the v6.4.3 opacity/alpha clamps are retained.
- **v6.4.6** (superseded) — AI WARP modes now render on real YouTube playback: per-mode compositor overlays (backdrop-filter bands, masked rings/wedges, blend tints) replace the SVG lens, which browsers silently ignore on the YouTube iframe.
- **v6.4.5** — attempted per-mode SVG pixel pipeline on YouTube (superseded by v6.4.6; it had no effect on the cross-origin iframe).
- **v6.4.4** — AI WARP FX boosted on the YouTube feed; canvas skewing smoothed.
- **v6.4.3** — all levels and alphas hard-clamped; Ghost, V-Sync and automations verified; Complete · Ready.
- **v6.4.2** — all 29 AI Warp modes boosted.
- **v6.4.1** — GC#(3) helper fixes; screen share and microphone confirmed.
- **v6.4.0** — first standalone installer.

## Keywords

River Lyle Reuveni, Imortalimp, creative coding, Web Audio, glitch engine, audio visualizer, digital art, WebView2, Windows installer
