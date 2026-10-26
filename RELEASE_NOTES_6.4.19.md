# AI WARP ENGINE 6.4.19 — Subtle Milkdrop Blends

Version 6.4.19 refines the Milkdrop-inspired FX and fixes suggested playlist playback in the standalone Windows app.

## Highlights

- Applies the 6.4.35 template's subtle Milkdrop blend: lower preset opacity, softer feedback, gentler polar waveforms and reduced bloom intensity.
- Keeps the 80 shuffled presets, audio reactivity, AI Warp/voice blending, and saved opacity/lock controls.
- Routes playlist-only recommendations through the standalone **AI WARP LENS**, including the lens audio tap for analyser-driven FX.
- Cancels a pending lens launch when another playback source starts, closes active lens playback on source changes, and stops embedded YouTube playback when it is superseded.
- Maintains the existing in-page YouTube player for browser builds and for ordinary video playback when the standalone lens is unavailable.

## Install

Download and run `AI-WARP-ENGINE-6.4.19-x64-setup.exe` on Windows 10/11 x64. The installer is self-contained and installs per-user without administrator access.

## Validation

- Installer file and product version: `6.4.19`.
- JavaScript syntax, app configuration JSON, and package/source parity checks passed.
- Browser smoke tests verified 80 presets with the subtle opacity range, the featured playlist's lens `videoseries` request, and both active and pending lens cleanup when synth playback starts.
- Lens audio continues through the standalone shell's existing process-loopback `AudioTap` when the lens frame/audio stream starts.

**Installer SHA-256:** `1796A70D7D883586B5BDB33A281217E7D1ABB53808AA4E5E6D8E058EE07D1E05`

For installation, controls, audio setup, effects and troubleshooting, [download the AI WARP ENGINE User Manual](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/download/v6.4.19/MANUAL.md).
