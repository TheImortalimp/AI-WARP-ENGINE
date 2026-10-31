# AI WARP ENGINE - v6.4.36

**A Windows visual synthesizer and audio-reactive media playground.**
Combine live media, generative artwork, 34 glitch voices, 29 warp effects and
audio-driven automation in one Canvas-based performance interface.

![AI WARP ENGINE artwork](assets/logo.png)

## Download and install

**[Download the v6.4.36 Windows x64 installer](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/download/v6.4.36/AI-WARP-ENGINE-6.4.36-x64-setup.exe)**

- Windows 10/11 x64 and Microsoft Edge WebView2 are required.
- The .NET runtime and bundled media are included.
- Run the installer, approve Windows UAC, and complete the wizard.
- The default destination is `Desktop\AI WARP ENGINE`, with desktop and Start
  Menu shortcuts and an uninstaller.
- **Elevation is required:** the installer applies system-pointer and
  theme/sound policy hardening. Review [About](ABOUT.md#cursor-and-theme-protection)
  before installing; this is not a visuals-only, no-admin installation.
- The installer is unsigned and may trigger SmartScreen.

Verify downloads against **SHA256SUMS.txt** in the
[release assets](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/tag/v6.4.36).
The release also includes a matching browser-app ZIP. Extract the entire folder,
including its media, and serve it over localhost rather than opening `file://`.

## What's fixed in 6.4.36

The previous release accidentally concatenated the app, showcase and YouTube
LENS documents. Duplicate canvases and controls started competing engines,
hid the initialization artwork and broke normal effect control.

- Restored the animated startup grid/rings and responsive effect playback.
- Restored the dedicated YouTube LENS player.
- Added single-instance JavaScript initialization, dependency/mount validation,
  a five-second configuration timeout and visible runtime/startup errors.
- Kept animation scheduling alive after transient rendering failures and
  restored canvas state after failed voice passes.
- Retained the Windows system-cursor and theme/pointer protections.
- Synchronized the HUD, app, installer and asset versions:
  **v6.4.36 - INIT + FX RESTORED - CURSOR GUARD**.

See [release notes](RELEASE_NOTES_6.4.36.md) and [changelog](CHANGELOG.md).

## Features

| Area | Included |
|------|----------|
| Sources | Generated visuals, local images/video/audio, YouTube URLs/playlists, shared screen/window/tab and microphone |
| Glitch mixer | 17 core voices plus 17 independent `GC#(3)` counterparts; per-voice level, size, depth and locks |
| Warp | 29 effects plus AI ZERO, including liquid, prism, vortex, kaleidoscope, twirl, RGB shift and neon edges |
| R3 overlays | Ghost Protocol: DIA, SPECTRUM, CRT, PLASMA, LED; V-Sync: Scanphasing, Color Harmony, Sharp |
| Audio | Bass/mid/treble/drive analysis, spectrum/waveform displays, microphone and built-in synth grooves |
| Automation | Auto Glitch, Continual Random, Automated Masters, Timed Auto, Soft-Eye and Crescendo |
| Presets | 80 machine presets, saved user presets, cross-fades and protected manual settings |
| Feedback | DROP THE MILK? with 80 original Milkdrop-inspired presets and lockable opacity |
| Output | Full-viewport B&W/Hue treatment and HUD-free Canvas PNG freeze frames |
| Showcase | Four bundled, original RAUM X-R synthesized tracks and artwork-driven visual sequences |

### YouTube and browser limitations

In the Windows app, a supporting **AI WARP LENS** window plays YouTube; the host
streams its rendered frames into the main Canvas and supplies audio analysis
through per-process loopback, with a system-loopback fallback. Switching sources
stops the lens.

A normal browser cannot read cross-origin YouTube iframe pixels. Use local media
or **Share Screen / Window** (`N`) for pixel-based effects and screenshots.
Embedding availability, internet access and codec support remain external
constraints; a fixed engine cannot make blocked YouTube videos playable.

## Quick controls

Start with the animated generated source, enable a mixer voice, and adjust its
level. Press `R` to randomize, `W` to cycle warp, `G` for Ghost, `V` for V-Sync,
`D` for Milkdrop, `Z` to clear voices, `S` for a freeze-frame PNG and `?` for help.
`M` toggles microphone monitoring, `N` shares a display source and `H` hides UI.
The Windows shell uses `F11` for fullscreen and `F5` for reload.

For setup and all controls, see the [user manual](MANUAL.md) and
[engine documentation](ai_warp_engine/README.md).

## Run the browser edition

From the repository root:

```powershell
python -m http.server 8000 --bind 127.0.0.1
```

Open <http://localhost:8000/ai_warp_engine/index.html>.
The aliases `index.html`, `ai_warp_engine.html` and `media_page.html` within the
app folder are identical. The repository-root `index.html` is a separate site
page, not the app entry point. Windows cursor policy enforcement and desktop
LENS capture are host-only features.

## Build and validate

Requires Node.js for tests, the .NET 8 SDK for the host, and
[NSIS](https://nsis.sourceforge.io/) for the installer.

```powershell
node --test tests\regressions.test.cjs
Set-Location shell
dotnet publish -c Release -r win-x64 --self-contained true -o publish
& "C:\Program Files (x86)\NSIS\makensis.exe" setup.nsi
```

Output: `shell\AI-WARP-ENGINE-6.4.36-x64-setup.exe`.
See [shell build documentation](shell/README.md).

Validation for this release: 11 dependency-free regression tests passed; browser
checks rendered all 34 voices and exercised all 30 mode selections without
JavaScript errors, including with the injected cursor guard. Windows publish
and NSIS packaging succeeded; all packaged HTML/JS/CSS/JSON matched source.
This is not an end-to-end certification of native capture or machine policies.

## Repository

- [ai_warp_engine/](ai_warp_engine/) - app HTML, JavaScript, CSS, configuration and bundled media.
- [shell/](shell/) - WinForms/WebView2 host, audio capture, cursor guards and NSIS installer.
- [tests/](tests/) - document, JavaScript, startup, render, LENS and version regression checks.
- [ABOUT.md](ABOUT.md) - project overview and protection boundaries.
- [DESCRIPTION.md](DESCRIPTION.md) - short GitHub/project description.

Settings live in `%LOCALAPPDATA%\AI-Warp-Engine\profile`. Optional
`AIWARP_PROFILE` selects a different profile, and `AIWARP_DEBUG_PORT` enables
local Chromium remote debugging when explicitly configured.

## License and credits

The software is dedicated to the public domain under
[CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) and
[The Unlicense](https://unlicense.org/); see [license](shell/LICENSE.txt).
The imortalimp.nl site content is CC BY 4.0. Third-party media and dependencies
retain their own rights and terms.

Created by River Lyle Reuveni (Imortalimp), with Arena.ai and GitHub Copilot.
The four original RAUM X-R tracks are dedicated to Stefan Betke (Pole), Daniel
Meteo, Burnt Friedman and Kit Clayton; they use no recordings by those artists
and are not affiliated with them. [Support the original album](https://pole-stefanbetke.bandcamp.com/album/r).
