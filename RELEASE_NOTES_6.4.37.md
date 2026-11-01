# FINAL NOVEMBER 2026 RELEASE

**AI WARP ENGINE 6.4.37 - Glitch Hardened & FX Fixed**

## Publication status

The [v6.4.37 release](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/tag/v6.4.37)
contains the browser app, full documentation and checksums.

**Windows installer and standalone app publication is pending.** The local
installer is built and version-verified, but its GPLv3 FFmpeg dependency lacks
verified complete corresponding source/build materials for linked libraries.
Those materials are requested in
[upstream issue #254](https://github.com/GyanD/codexffmpeg/issues/254).
An FFmpeg source commit alone is not complete coverage of its static dependencies.
Do not interpret this release title as confirmation that Windows binaries are
available or redistribution requirements have been met.

## Downloads and deployment

| Asset | Status | Purpose |
|---|---|---|
| `AI-WARP-ENGINE-6.4.37-browser.zip` | Published | Complete `ai_warp_engine` browser folder |
| `AI-WARP-ENGINE-6.4.37-documentation.zip` | Published with these notes | Manual, build guide, release notes and notices |
| `SHA256SUMS.txt` | Published | SHA256 of public ZIP assets |
| `AI-WARP-ENGINE-6.4.37-x64-setup.exe` | Local only, publication pending | Elevated Windows installer |
| Windows app bundle | Not published | Complete app/runtime/assets required; EXE alone is insufficient |

Extract the browser ZIP beside your existing website pages. Serve over HTTPS,
or localhost for testing; open `ai_warp_engine/index.html`. Keep all runtime
scripts, configuration, CSS and the ten bundled audio/artwork files.
The ZIP excludes website-root `home.html` and `index.html`, obsolete ZIPs,
the legacy `web` bundle, native shell and FFmpeg. App aliases remain included.
GitHub automatic source archives retain the repository's existing landing pages;
use the browser ZIP for an app-only upload.

## FX and playback fixes

- Synchronized completed-frame Canvas presentation prevents mid-pass exposure.
- Separate warp composition, gap coverage and 240 ms transitions preserve FX
  across mode changes without deliberately flattening strobe/glitch rhythms.
- Integrated warp timing prevents phase jumps when speed changes.
- Last-good-frame recovery and visible errors keep the render loop alive.
- Local-video buffering retains the clean source frame, not an already-effected
  feedback frame.
- Windows unsupported media conversion uses a separate offline FFmpeg process,
  H.264/AAC MP4 or AAC M4A output, cancellation and generation guards.
  Originals remain unchanged; cache hits avoid unnecessary conversion.

## Platform and control boundaries

Browser codecs still apply. Convert unsupported AVI locally before browser
playback. Automatic conversion is Windows-only. YouTube iframe pixels cannot
be read in the browser: share a tab/screen for readable frames and supported
capture audio. The native app supplies its own supporting LENS capture.

The main app bundles Three.js and blocks remote scripts with a local-only script
policy. External-browser/new-window launches are blocked inside the app.
Windows navigation, native bridge, device permission and release-debugging
boundaries are restricted. Accessibility remains available.

No Defender hooks/scans, physical-input-only guarantee or protection against
same-user/admin malware or Windows remote-control software is claimed.

## Windows setup, privacy and prerequisites

Windows 10/11 x64 and WebView2 are required. .NET runtime files are bundled.
The unsigned installer requests UAC and installs to the user's desktop by
default. It applies existing cursor/theme/sound Windows policies; review
[About](ABOUT.md#cursor-and-theme-protection) before local installation.
An app-only bundle must include all DLLs, `app` and `tools`, not just the EXE.

App settings and converted copies live in
`%LOCALAPPDATA%\AI-Warp-Engine\profile` unless `AIWARP_PROFILE` overrides it.
Conversion stays offline; online YouTube and playlist features contact their
services. Source switching cancels owned conversion work.
Uninstall does not automatically revert Windows policies or remove the user
profile. Back up presets/settings before manual profile cleanup.

## Validation

- 25 JavaScript app/FX/local-playback tests passed.
- 22 native app-control assertions passed.
- Eight native media-conversion checks passed, including corrupt inputs,
  cancellation, original preservation and valid converted audio/video.
- Prior native/browser checks sampled 2,610 FX frames across 29 modes and three
  strengths without transparent/untreated frames.
- Browser ZIP contents and uploaded SHA256 verified.
- Local EXE version: `6.4.37.0`; installer version: `6.4.37`.

These are targeted tests, not a Windows security or legal certification.

## Documentation

[README](README.md) | [Manual](MANUAL.md) | [About](ABOUT.md) |
[Description](DESCRIPTION.md) | [Changelog](CHANGELOG.md) |
[Browser guide](ai_warp_engine/README.md) | [Windows build guide](shell/README.md) |
[Third-party notices](THIRD_PARTY_NOTICES.md)
