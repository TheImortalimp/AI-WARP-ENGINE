# ⚡ AI WARP ENGINE — Windows App (R3 Canvas Edition v6.4)

<p align="center">
  <img src="assets/logo.png" alt="AI WARP ENGINE 6.4.1" width="720">
</p>

A public-domain glitch visual synthesizer and audio-reactive media playground, packaged as a standalone Windows app with a one-click installer.

**Status: Complete · Ready** — download the latest installer from the [Releases page](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/latest).

## Install

1. Download `AI-WARP-ENGINE-6.4.3-x64-setup.exe` from the latest release.
2. Run it, accept the licence terms, and finish the wizard. No admin rights are needed.
3. The app installs to `Desktop\AI WARP ENGINE`, with a launcher shortcut, a Start Menu entry and an uninstaller.

Requires Windows 10/11 x64 with the Microsoft Edge WebView2 runtime (included with current Windows). The .NET runtime is bundled.

## Features

- **34 glitch voices** — 17 core voices plus 17 Glitch Revision 3 voices (`GC#(3)`).
- **29 AI Warp modes** — liquid, prism, vortex, kaleidoscope, analog CRT, 17 NewBlue-inspired modes and more.
- **Ghost Protocol R3** (DIA, SPECTRUM, CRT, PLASMA, LED) and **V-Sync R3** (Scanphasing, Color Harmony, Sharp).
- **Display capture and microphone** — share a tab, window or screen; feed the audio-reactive analyser.
- **YouTube Warp Bus** with the SVG Warp Lens, local image/video/audio, 80 machine presets, user presets and automation.

### Hotkeys

| Key | Action |
|-----|--------|
| `N` | Toggle display capture (screen share) |
| `M` | Toggle microphone |
| `Y` | Load the featured YouTube playlist |
| `E` | Cycle Audio Reactive Ear (AUTO / AUDIO / CANVAS) |
| `F11` | Toggle fullscreen |
| `F5` | Reload |

Full engine documentation: [ai_warp_engine/README.md](ai_warp_engine/README.md).

## Repository layout

| Path | Contents |
|------|----------|
| `ai_warp_engine/` | The web app (HTML, JS, CSS) |
| `shell/` | WinForms + WebView2 host, NSIS installer script, icon, `LICENSE.txt` |
| `home.html`, `index.html` | imortalimp.nl site pages |

## Build from source

Requires the .NET 8 SDK and NSIS.

```powershell
cd shell
dotnet publish -c Release -r win-x64 --self-contained true -o publish
& "C:\Program Files (x86)\NSIS\makensis.exe" setup.nsi
```

Settings and the web profile are stored in `%LOCALAPPDATA%\AI-Warp-Engine`. For debugging, set `AIWARP_DEBUG_PORT` to expose a Chromium remote-debugging port.

## Licence

The software is dedicated to the public domain under [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) and [The Unlicense](https://unlicense.org/). The imortalimp.nl site content is CC BY 4.0. See [shell/LICENSE.txt](shell/LICENSE.txt).

## Credits

River Lyle Reuveni ([imortalimp.nl](https://imortalimp.nl) · [@TheImortalimp](https://github.com/TheImortalimp)), [Arena.ai](https://arena.ai) Agent Mode, and GitHub Copilot.
