# About AI WARP ENGINE

**6.4.38.21 ULTIMATE**

AI WARP ENGINE is a visual synthesizer and audio-reactive media playground.
It combines a plain HTML/JavaScript/Canvas app with a Windows WinForms/WebView2
host. The same app files support browser use, while the host supplies desktop
YouTube LENS frame capture, audio loopback, native media conversion and
Windows-specific protections.

## 6.4.38.21 ULTIMATE

The confluence release. One build carries the 6.4.36 effect banks, the 6.4.37
FX-flow hardening, the 6.4.37.1 Freeze Frame and the native Windows media
backend of 6.4.38.x. Each claim below is backed by a test in this repository.

- **6.4.36 effects, proven and pinned.** The 34 voices (17 core and 17 `GC#(3)`),
  the 29 warp modes plus AI ZERO, the Ghost Protocol and V-Sync rails and the
  80 Milkdrop presets come from the 6.4.36 sources. `glitch_engine.js`,
  `r3_gc3_factories.js`, `r3_ghost_vsync.js` and `yt_lens.html` are
  byte-identical to 6.4.36 and pinned by SHA-256. The engine checks a frozen
  effect registry at startup, and `attestEffectBanks()` re-hashes the files the
  page was actually served.
- **One pipeline.** Rendering, conversion, building and testing use only the
  app's own Canvas/WebGL code and Windows components. No external media tool is
  bundled, installed or launched; a test scans the whole tree to keep it that way.
- **Aligned canvases.** The lens canvas, all nine effect buffers, the Tech Artist
  WebGL buffer and its resolution uniform, and the fullscreen layers share one
  size at every window size and display scaling. Resizing redraws the last
  picture into the new buffers instead of blanking the lens for a frame.
- **Freeze Frame without the HUD.** The still contains the effects canvas, the
  LENS/media layer underneath, CRT scanlines, vignette and the page filter once.
  The whole HUD is hidden first: bars, mixer, badges, the HIDE U.I. tab, drawers
  and dialogs, including nodes that were mid-transition or force their own
  visibility. A magenta-sentinel test proves nothing from the HUD reaches the PNG.
- **Steady Tech Artist.** The WebGL raymarcher now draws at the same size as the
  lens (it used to crop and drift at 125% scaling and above). If WebGL cannot
  start, the 2D rays take over and WebGL is retried at most every 5 seconds.
- **Notifications you can see.** The HUD notification banner (blocked links,
  playlist and LENS errors, confirmations) used to sit inside the effects layer,
  beneath the control bars, and was hidden by them. It is now a page-level HUD
  node above the bars, still hidden with the collapsed HUD and in Freeze Frame.
- **Gold reversed-R icon, crisp at every size**, and the .NET tribute and licence
  pages in the installer.

The title-adjacent HUD notice reads **6.4.38.21 ULTIMATE**.

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

## Native media

The Windows host converts local media that the browser engine cannot decode,
using Windows MediaTranscoder and the codecs installed on the PC, to H.264/AAC
MP4 or AAC M4A copies in the profile's `media-cache`. Originals are never
overwritten. Unsupported or corrupt files produce an explicit error. This is not
a promise that every AVI or codec plays: it depends on the Windows codecs
present. .NET is bundled; WebView2 and the Windows media components are
prerequisites, and the native label describes the media backend, not an
all-native UI. The earlier 6.4.37 and 6.4.37.1 Windows binaries stay withheld
pending source review; see their release notes.

## Release history

- **6.4.38.2 - FINAL NOVEMBER FIXED RELEASE.** Native installer/app with the
  crisp gold icon, .NET tribute and licence pages, and corrected installer finish
  text and startup bounds. An elevated installation was tested; all 288
  installed app files matched the published manifest.
- **6.4.38.1.** First native Windows media edition.
- **6.4.37.1.** Freeze Frame captures all FX and excludes the HUD.
- **6.4.37.** Continuous FX flow: synchronized frames, 240 ms mode transitions
  and failure recovery. App navigation and native controls are restricted,
  remote app scripts and release debugging are disabled, and external browsers
  cannot launch. Accessibility and existing cursor protections remain; no
  Defender scans or physical-input-only guarantee are included.
- **6.4.36.** Startup repair. The 6.4.35 packaging regression joined independent
  HTML documents, duplicating controls and creating competing engines; this
  release separated the app aliases, showcase fragment and LENS player again.
  Startup is single-instance and validates required scripts and media mounts.

## Website

The website ZIP adds the complete browser folder, the gold favicon and updated
root imortalimp.nl home/index showcases while preserving the other site
sections. Back up the root pages before extracting it into the document root;
publishing on GitHub does not deploy the live website. Browser codec limits
remain distinct from native conversion.

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
- App-layer hardening cannot stop malware running with the user's or an
  administrator's privileges.

## Credits and license

Created by River Lyle Reuveni (Imortalimp), with Arena.ai and GitHub Copilot.
Software is public domain under CC0 1.0 / The Unlicense; the site and third-party
assets retain their respective terms.

The original RAUM X-R synthesized tracks honor Stefan Betke (Pole), Daniel
Meteo, Burnt Friedman and Kit Clayton without using their recordings or implying
affiliation. [Support the original album](https://pole-stefanbetke.bandcamp.com/album/r).

[Installation and build guide](README.md) |
[User manual](MANUAL.md) |
[Release notes](RELEASE_NOTES_6.4.38.21.md) |
[Third-party notices](THIRD_PARTY_NOTICES.md)
