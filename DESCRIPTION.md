# AI WARP ENGINE - description

Current version: **6.4.38.21 ULTIMATE**.
Windows MediaTranscoder handles supported conversions through installed OS
codecs. .NET is bundled; WebView2 and Windows media components are required.
This is not universal AVI support or an all-native UI.
See [release notes](RELEASE_NOTES_6.4.38.21.md).

## GitHub About description

AI WARP ENGINE: Windows visual synthesizer with 34 glitch voices, 29 warp effects and YouTube LENS. 6.4.38.21 ULTIMATE: proven 6.4.36 effects, aligned canvases, HUD-free Freeze Frame and native Windows media.

## Overview

AI WARP ENGINE is a Canvas-based visual performance app for Windows 10/11 x64,
with a matching browser edition. Layer generative visuals, local media, YouTube
or shared-display frames with independently controlled glitch voices, warp
modes, R3 Ghost Protocol/V-Sync and audio-driven automation.

The app includes 80 machine presets, saved user presets, Milkdrop-inspired
feedback, built-in synth grooves, four original RAUM X-R tracks, voice/master
locks, full-viewport B&W/Hue controls and Canvas PNG freeze frames.

6.4.38.21 ULTIMATE keeps the 6.4.36 effect banks byte-for-byte (pinned by
SHA-256 and re-checked at runtime), keeps the lens canvas, every effect buffer
and the fullscreen layers aligned at any window size or display scaling, and
makes Freeze Frame exclude the entire HUD. It builds on the 6.4.37 FX flow
(synchronized completed frames, 240 ms transitions, last-good-frame recovery)
and its navigation and native-control hardening: remote app scripts, release
debugging and external-browser launches are disabled, while the Windows
system-pointer protections and accessibility remain. The HUD notice reads
`6.4.38.21 ULTIMATE`.

The installer requires UAC elevation and applies cursor/theme/sound policy
hardening; it is unsigned. Browser-only usage does not apply Windows policies
and uses display sharing for readable YouTube pixels.

- [v6.4.38.21 ULTIMATE](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/tag/v6.4.38.21):
  installer, app and browser ZIPs, source, documentation, website bundle and checksums.
- [Previous v6.4.38.2 FINAL NOVEMBER FIXED RELEASE](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/tag/v6.4.38.2)
- [v6.4.36 release](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/tag/v6.4.36):
  the source of the pinned effect banks. The 6.4.37 and 6.4.37.1 Windows binaries
  remain withheld pending source review; see [third-party notices](THIRD_PARTY_NOTICES.md).
- [README](README.md) - installation, features, controls and build commands.
- [Manual](MANUAL.md) - using the app.
- [About](ABOUT.md) - project goals and protection boundaries.
- [Changelog](CHANGELOG.md) - fixes and validation.
- [Website](https://imortalimp.nl/ai_warp_engine/)

Software: CC0 1.0 / The Unlicense. Third-party assets retain their own rights.
