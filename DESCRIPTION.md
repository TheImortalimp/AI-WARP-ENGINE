# AI WARP ENGINE — Description

**AI WARP ENGINE (R3 Canvas Edition v6.4)** is a public-domain visual synthesizer, audio-reactive media playground and live Canvas glitch suite, shipped as a standalone Windows app.

- **Author:** River Lyle Reuveni (Imortalimp), [imortalimp.nl](https://imortalimp.nl)
- **Version:** 6.4 (installer release v6.4.18) — **Complete · Ready**
- **Platform:** Windows 10/11 x64 (WebView2 host, .NET bundled)
- **Install location:** `Desktop\AI WARP ENGINE` (per-user, no admin)
- **Licence:** CC0 1.0 / The Unlicense (software); CC BY 4.0 (imortalimp.nl site content)

## What it does

Takes a live source and runs it through a real-time Canvas effects pipeline, driven by an audio-reactive analyser (bass / mid / treble / drive).

**Sources:** built-in generative visuals, local image / video / audio files, YouTube (playlist or URL), a shared tab / window / screen (display capture, `N`), and microphone audio (`M`).

**Effects:**
- 34 independently controllable glitch voices: 17 core (Neon Grad, Scanlines, Data Bands, Flash, HSL Rainbow, ASCII, Font Burn, Strobe, Gemini Bomb, Tech Artist, VHS Glitch, Data Bomb, Ink Glyphs, Matrix ASCII, Matrix Chord, B&W Invert, Hue/Color Cycle) plus 17 Glitch Revision 3 `GC#(3)` counterparts.
- 29 AI Warp modes: 12 algorithmic (liquid, prism, breathe, fold, aurora, vortex, cybergrid, hyperdrive, kaleidoscope, quantum, ripple, analog CRT) and 17 NewBlue-inspired (roll, active cam, shear, wiggle, zoom blur, video tile, twirl, light ring, earthquake, water drop, RGB shift, quad mirror, fish eye, pinch, solarize, cathode beam, neon edges), plus a compositor-level SVG Warp Lens for YouTube.
- Ghost Protocol R3 (DIA, SPECTRUM, CRT, PLASMA, LED) and V-Sync R3 (Scanphasing, Color Harmony, Sharp) overlays, with Soft-Eye, Crescendo and 29 mode-aware FX blend profiles.
- **DROP THE MILK?** — original Milkdrop-inspired Canvas feedback with audio-reactive polar waves, liquid-colour blooms and 80 shuffled presets; presets blend with active warp modes/voices, and opacity automation has a persistent user lock.

**Automation:** Auto Glitch, Continual Random, Automated Masters, Timed Automation, Wired mode, Soft-Eye and Crescendo.

**Control:** 80 machine presets plus saved user presets with smooth cross-fades, a live mixer, and keyboard hotkeys (`W` warp, `G` ghost, `V` V-Sync, `A` auto, `R` randomise, `T` timed, `C` crescendo, `D` Milkdrop, `Z` zero, `S` freeze, `E` ear, `H` hide UI, `1`–`9` voices, `?` help).

## Mode-aware effects

The 29 AI Warp modes each have a deterministic blend profile that tunes Ghost Protocol / V-Sync color, split, scan, warp and automation pacing. The profiles also blend R3 factory effects, Canvas warp strength, Hard Light, Soft-Eye and audio-ear response without changing master levels or overriding protected voice intent.

## DROP THE MILK?

Enable the DROP THE MILK? toolbar button or press `D`. The effect composites a shuffled deck of 80 original feedback presets over the live Canvas, using analyser-driven polar waveforms, colour blooms and smooth feedback zoom/rotation. Presets respond to the active AI Warp profile and enabled FX voices. The MILKDROP opacity slider sets blend strength; leave it unlocked for preset-driven opacity changes, or lock it to retain your chosen level. Both opacity and lock preference persist across reloads.

## Release history

- **v6.4.18** — Added DROP THE MILK? Milkdrop-inspired audio-reactive Canvas feedback, 80 shuffled presets, mode/voice blending and persistent opacity lock.
- **v6.4.17** — Mode-aware FX blending for all 29 AI Warp modes, deterministic automation and coordinated Ghost / V-Sync / R3 / Canvas effects.
- **v6.4.16** — Final: YT LENS audio captured with the direct per-process audio driver (system loopback fallback) to drive the audio monitor, cadence and EAR modes.
- **v6.4.14** — Fixed local-media pause button and YT LENS frames lost after switching to local media and back.
- **v6.4.13** — Window keeps its border and stays resizable/draggable while the lens runs (no forced full screen).
- **v6.4.12** — Lens window hidden off-screen, auto full screen, audio monitor/cadence/EAR driven by picture motion on the lens.
- **v6.4.11** — YT LENS: YouTube plays in a second AI WARP LENS window and its real pixels feed the canvas pipeline.
- **v6.4.10** — YouTube overlay bridge removed; the effect engine runs exactly as on imortalimp.nl/ai_warp_engine.
 - **v6.4.9** — site engine, Ghost Protocol and V-Sync restored verbatim (no boosts, no added clamps); YouTube overlay lives in the separate yt_overlay_bridge.js.
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
