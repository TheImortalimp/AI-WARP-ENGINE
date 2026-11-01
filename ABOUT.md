# About AI WARP ENGINE

**6.4.38.1 COMPLETE (ALL WINDOWS x64 NATIVE) -noffmpeg_(~Gyan-D).**

The current edition uses Windows MediaTranscoder and installed Windows codecs
without FFmpeg. .NET is bundled; WebView2 and OS media components are prerequisites.
The native label describes the media backend, not universal file support or
an all-native UI. No affiliation with GyanD is implied.

AI WARP ENGINE is a visual synthesizer and audio-reactive media playground.
It combines a plain HTML/JavaScript/Canvas app with a Windows WinForms/WebView2
host. The same app files support browser use, while the host supplies desktop
YouTube LENS frame capture, audio loopback and Windows-specific protections.

## Creative engine

The mixer exposes 34 independent voices: 17 core effects and 17 `GC#(3)` rails.
Combine these with 29 warp effects or AI ZERO, R3 Ghost Protocol and V-Sync,
80 machine presets, user presets and Milkdrop-inspired feedback. Audio analysis,
timed automation and Soft-Eye/Crescendo can animate the stack; locks preserve
manual intent against automation.

Sources include the animated initialization grid/rings, local media, shared
displays, YouTube, microphones and original bundled synth/RAUM X-R material.
The complete web viewport can receive B&W/Hue treatment, while Canvas
freeze frames exclude the HUD.

## What changed in 6.4.37

Continuous FX use synchronized frames, smooth mode transitions and failure
recovery. The earlier 6.4.37 Windows build used bundled FFmpeg; current 6.4.38.1
converts supported files through Windows codecs and reports unsupported inputs.
App navigation and native controls are restricted, remote app
scripts and release debugging are disabled, and external browsers cannot launch.
Accessibility and existing cursor protections remain; no Defender scans or
physical-input-only guarantee are included.

The title-adjacent HUD notice reads:
**6.4.38.1 COMPLETE (ALL WINDOWS x64 NATIVE) -noffmpeg_(~Gyan-D)**.

### Previous startup repair in 6.4.36

The 6.4.35 packaging regression joined independent HTML documents together,
duplicating controls and creating competing engines. This release separates
the app aliases, showcase fragment and LENS player again. Startup is
single-instance and validates required scripts/media mounts; configuration
loading is bounded, errors are surfaced, and transient rendering failures
cannot cancel animation scheduling.

The previous title-adjacent HUD notice read:
**v6.4.36 - INIT + FX RESTORED - CURSOR GUARD**.

## Cursor and theme protection

The Windows host retains the system-pointer guard: page scripts cannot install
custom pointer images, while normal Windows pointer/text/resize semantics
remain available. Both app and LENS views receive the guard and theme/pointer
download/navigation restrictions.

The elevated installer also applies cursor and theme/sound policy hardening.
This changes Windows settings, not just app appearance. The scripts and runtime
guard are available for inspection in [shell/](shell/); existing registry
backups are managed by those scripts. An administrator can change or remove
policies. These protections are not a malware scanner, code signing, or proof
that a machine is safe. They have not been certified end-to-end on every
Windows configuration.

The installer is unsigned; SmartScreen may warn. Browser-only usage does not
enforce Windows settings.

## Technical boundaries

- Cross-origin YouTube iframe pixels cannot be read by browser JavaScript.
  Desktop LENS capture or browser display sharing supplies readable frames.
- Native audio may fall back from per-process capture to system loopback.
- Online video embedding, external services and media codecs can be unavailable.
- Settings use the local WebView2 profile; microphone/display sources require
  appropriate permissions.

## Credits and license

Created by River Lyle Reuveni (Imortalimp), with Arena.ai and GitHub Copilot.
Software is public domain under CC0 1.0 / The Unlicense; the site and third-party
assets retain their respective terms.

The original RAUM X-R synthesized tracks honor Stefan Betke (Pole), Daniel
Meteo, Burnt Friedman and Kit Clayton without using their recordings or implying
affiliation. [Support the original album](https://pole-stefanbetke.bandcamp.com/album/r).

[Installation and build guide](README.md) |
[User manual](MANUAL.md) |
[Release notes](RELEASE_NOTES_6.4.37.md) |
[Third-party notices](THIRD_PARTY_NOTICES.md)
