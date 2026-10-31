# ⚡ AI WARP ENGINE — Windows App (R3 Canvas Edition v6.4.27)

<p align="center">
  <img src="assets/logo.png" alt="AI WARP ENGINE 6.4.27" width="720">
</p>

A public-domain glitch visual synthesizer and audio-reactive media playground, packaged as a standalone Windows app with a one-click installer.

**Current release: v6.4.27** — download the Windows installer from the [GitHub release](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/tag/v6.4.27) or use the app-matched browser edition in `ai_warp_engine/`.

## Install

1. Download and run `AI-WARP-ENGINE-6.4.27-x64-setup.exe` from the [v6.4.27 release](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/tag/v6.4.27).
2. Run it, accept the licence terms, and finish the wizard. No admin rights are needed.
3. The app installs to `Desktop\AI WARP ENGINE`, with a launcher shortcut, a Start Menu entry and an uninstaller.

Requires Windows 10/11 x64 with the Microsoft Edge WebView2 runtime (included with current Windows). The .NET runtime is bundled, so nothing else needs installing. The installer is about 140 MB because it bundles the four Raum X-R tracks.

## What it does

AI WARP ENGINE takes a live source and runs it through a real-time Canvas effects pipeline:

- **Sources:** built-in generative visuals, local image / video / audio files, YouTube (playlist or URL), a shared tab / window / screen (display capture), and microphone audio.
- **Effects:** 34 glitch voices, 29 AI Warp modes, Ghost Protocol R3 and V-Sync R3 overlays.
- **Audio reactivity:** a bass / mid / treble / drive analyser (from the mic, the playing audio or the canvas itself) drives the warps, voices and overlays.
- **Automation:** Auto Glitch, Continual Random, Automated Masters, Timed Automation, Wired mode, Soft-Eye and Crescendo.
- **Presets:** 80 machine presets and your own saved user presets, with smooth cross-fades between them.

### Full app-frame inversion (v6.4.27)

The B&W control now converts the final composition to grayscale and inverts it after all source and effect layers have rendered. Hue/Color Cycle similarly rotates the complete rendered spectrum. The compositor now covers the entire app viewport, including the captured YT LENS feed, initialization artwork, Raum X-R showcase, easter eggs, HUD, controls, dialogs and overlays. Both the browser app and the Windows WebView2 host share this implementation; native Windows title bars and frame chrome remain outside the web viewport. Hard Light stays limited to the effects canvas, and freeze-frame screenshots preserve the page-level color filter. Previous LENS tuning and automatic playlist extraction remain included.

See the [v6.4.27 release notes](RELEASE_NOTES_6.4.27.md) for details.

## Features

### 34 glitch voices

Each voice has its own on/off, level, size and depth.

- **17 core voices:** Neon Grad, Scanlines, Data Bands, Flash, HSL Rainbow, ASCII, Font Burn, Strobe, Gemini Bomb, Tech Artist, VHS Glitch, Data Bomb, Ink Glyphs, Matrix ASCII, Matrix Chord, B&W Invert, Hue/Color Cycle.
- **17 Glitch Revision 3 voices** (`GC#(3)`): the same set rebuilt from the R3 factory bank, so each can run next to its original.

### 29 AI Warp modes

| # | Modes |
|---|-------|
| 1–12 | liquid, prism, breathe, fold, aurora, vortex, cybergrid, hyperdrive, kaleidoscope, quantum, ripple, analog_crt |
| 13–29 (NewBlue-inspired) | newblue_roll, active_cam, shear, wiggle, zoom_blur, video_tile, twirl, light_ring, earthquake, water_drop, rgb_shift, quad_mirror, fish_eye, pinch, solarize, cathode_beam, neon_edges |

- The **AI WARP Strength** slider scales every mode; **Master Opacity** scales the whole stack.
- On YouTube sources the warp runs through the original **Warp Bus** exactly as on imortalimp.nl/ai_warp_engine: CSS 3D transforms, skew and clip-path tears, the SVG Warp Lens and the voice filter chain, with the glitch canvas voices layered above. The YouTube iframe is cross-origin, so its pixels cannot be read; in the desktop app the video therefore plays in a second **AI WARP LENS** window whose rendered frames are streamed to the main canvas, so every voice and warp mode acts on the real YouTube pixels (the lens window is parked off-screen and the app keeps its normal resizable window frame (F11 for full screen); keep it open, its audio plays from there, and the audio monitor / cadence / EAR modes are driven by the real lens audio, tapped straight from the app's own audio via the Windows direct per-process audio driver, with a system-loopback fallback). In a normal browser use **Share Screen / Window** (`N`).

### Ghost Protocol R3 and V-Sync R3

- **Ghost Protocol:** OFF, DIA, SPECTRUM, CRT, PLASMA, LED. Each has an adjustable level and follows the audio and the source palette.
- **V-Sync:** OFF, **SCANPHASING**, **COLOR HARMONY**, **SHARP** (the Full mode), each with its own level, rolling scan bars, tears and tracking.
- **Soft-Eye** (OFF, CALM, SURGE, CRESCENDO) ramps intensity over time, and **Crescendo** fires a short burst.

### Mode-aware FX (v6.4.17)

Each of the 29 AI Warp modes now has a deterministic blend profile that tunes Ghost Protocol and V-Sync color, split, scan, warp and automation pacing. R3 factory effects, Canvas warp strength, Hard Light, Soft-Eye and audio-ear response follow the active profile while preserving the existing master levels and user-protected voice intent.

### DROP THE MILK? subtle blends (v6.4.19)

The Milkdrop-inspired Canvas feedback now uses the subtler blends from the 6.4.35 web template: softer feedback, waveforms and blooms with lower preset opacity, while keeping all 80 shuffled presets, audio response, mode/voice blending and opacity locking.

Suggested/featured playlists now use the standalone AI WARP LENS path, with the lens audio feeding the analyser. Starting another playback source closes the lens and stops its audio.

See the [v6.4.19 release notes](RELEASE_NOTES_6.4.19.md) for details and installer checksum.

### RAUM · X-R showcase (v6.4.20)

The old Pole · R showcase, whose remote tracks could not play, is replaced by four **original, fully synthesized** tracks bundled inside the app, so they play offline and feed the audio-reactive analyser like any other source:

| # | Title | Length | Style |
|---|---|---|---|
| 1 | pole - Я - raum 1 - variant X-R | 7:15 | ripple flow + jungle breakcore with tempo drops, beat silences and crescendos |
| 2 | pole - Я - raum 2 - variant X-R | 8:05 | ripple flow + jungle breakcore with tempo drops, beat silences and crescendos |
| 3 | pole - Я - raum 3 - variant X-R | 9:03 | ambient harmonic beats and glitches under glass-bell drones |
| 4 | pole - Я - raum 4 - variant X-R | 9:03 | ambient harmonic beats and glitches under glass-bell drones |

Open them from the **RAUM · X-R (4)** button. The drawer shows each track's cover art, the back cover with the tracklist, and the thanks and dedication panel.

**Credits:** created by GitHub Copilot (AI) as an original work, prompted and directed by River Lyle Reuveni (Imortalimp). Dedicated to Stefan Betke (Pole) and the artists of the 1998 album *R* on DIN: Daniel Meteo, Burnt Friedman and Kit Clayton. These tracks contain no samples or recordings by them and are not affiliated with them. Please support the original album: <https://pole-stefanbetke.bandcamp.com/album/r>.

**Artwork slideshow and exclusive glitch sequences (v6.4.21):** while a RAUM · X-R track plays, the main canvas switches to a slideshow of the four covers and the back cover, replacing the init artwork or any other source, and is warped by original glitch sequences that follow the track clock and its audio: TEAR, GRID, MIRROR (backwards-Я fold), SMEAR (datamosh trail), COLUMNS and RIPPLE (glass-bell halos, ambient tracks), plus a decoding caption, a giant reversed Я that surfaces during beat silence, a reversal flash when the beat re-enters, and a hidden SIGNATURE lattice every 64 seconds of track time. Pausing or starting any other source restores the previous visuals.

See the [v6.4.21 release notes](RELEASE_NOTES_6.4.21.md) for details and the installer checksum.

### FX automation and color tuning (v6.4.22)

The bottom dock now provides fine-tune sliders and separate locks for B&W inversion strength and Hue/Color Cycle offset. **ROLL FX**, board randomization, and each preset auto-cycle roll randomized values only for unlocked controls. Auto Glitch receives an independent 50/50 on/off roll with board randomization and each preset auto-cycle. Core and GC#(3) voices remain independently controllable and retain subtle rendering differences while automation continues to select complementary pairs.

See the [v6.4.22 release notes](RELEASE_NOTES_6.4.22.md) for details and the installer checksum.

### YouTube playlist vault (v6.4.23)

The featured playlist is now pinned as a protected built-in entry in the app’s YouTube knowledgebase and remains visible even if RSS extraction is temporarily unavailable. The vault displays saved playlists with direct **PLAY** and **EXTRACT** actions. Extraction parses Atom/XML entries by namespace, uses bounded feed retries, and merges duplicate requests instead of launching competing fetches. Playlist-only input starts direct YouTube playlist playback immediately while extracted videos load in the background.

YouTube’s public playlist feed may expose only a recent subset of a playlist’s videos; use **PLAY** to browse the complete playlist on YouTube. When feed access is blocked by network/CORS, the saved playlist remains available to play directly.

See the [v6.4.23 release notes](RELEASE_NOTES_6.4.23.md) for details and the installer checksum.

### LENS FX tuning and automatic extraction (v6.4.24)

The bottom-dock B&W and Hue controls now activate their paired core and GC#(3) voices when adjusted, making strength and spectrum changes visible on the LENS Canvas without first enabling the voices elsewhere. Monochrome and hue-spectrum previews track the controls. Playing a saved playlist card or importing a playlist now also starts feed extraction in the background; direct YouTube playlist playback remains available while extraction runs.

See the [v6.4.24 release notes](RELEASE_NOTES_6.4.24.md) for details.

### Capture and audio

- **Display capture** (`N`) shares a tab, window or screen as the source.
- **Microphone** (`M`) feeds the audio-reactive analyser.
- **Audio Reactive Ear** (`E`) cycles AUTO, AUDIO and CANVAS.
- Camera, microphone and display capture are auto-allowed for the app's own origin only.

## Hotkeys

Press `?` in the app for the in-app shortcut list. Hotkeys are ignored while a text field is focused.

| Key | Action |
|-----|--------|
| `Space` | Play / pause the current audio or video (or the built-in synth) |
| `W` | Cycle AI Warp mode |
| `G` | Ghost Protocol on / off / cycle |
| `V` | V-Sync on / off / cycle |
| `A` | Auto Glitch |
| `R` | Randomise |
| `T` | Timed Automation |
| `C` | Crescendo |
| `D` | Toggle DROP THE MILK? feedback FX |
| `Z` | Zero everything |
| `S` | Freeze frame |
| `M` | Toggle microphone |
| `N` | Toggle display capture (screen share) |
| `Y` | Load the featured YouTube playlist |
| `E` | Cycle Audio Reactive Ear |
| `H` | Hide / show the interface |
| `1`–`9` | Toggle glitch voices 1–9 |
| `?` | Show the shortcut list |
| `F11` | Toggle fullscreen (app window) |
| `F5` | Reload (app window) |

## Repository layout

| Path | Contents |
|------|----------|
| `ai_warp_engine/` | The web app: `index.html`, `media_page.js` (UI and presets), `ai_warp_engine.js` (engine and warp modes), `glitch_engine.js` (presets), `r3_gc3_factories.js` (R3 voices), `r3_ghost_vsync.js` (Ghost / V-Sync), `REVISION_NOTES.md` |
| `shell/` | WinForms + WebView2 host (`Program.cs`), NSIS installer script (`setup.nsi`), icon, `LICENSE.txt` |
| `assets/` | README artwork |
| `home.html`, `index.html` | imortalimp.nl site pages |
| `DESCRIPTION.md` | Short project description |

Full engine documentation: [ai_warp_engine/README.md](ai_warp_engine/README.md).
For the standalone app’s setup and controls, see the [user manual](MANUAL.md).

## Build from source

Requires the .NET 8 SDK and [NSIS](https://nsis.sourceforge.io/).

```powershell
cd shell
dotnet publish -c Release -r win-x64 --self-contained true -o publish
& "C:\Program Files (x86)\NSIS\makensis.exe" setup.nsi
```

The web app is plain HTML / JS / CSS, so you can also run it in any Chromium browser by serving `ai_warp_engine/` over `http://localhost`.

### Data and debugging

- Settings and the web profile are stored in `%LOCALAPPDATA%\AI-Warp-Engine\profile`.
- Optional environment variables, unused unless set:
  - `AIWARP_DEBUG_PORT` exposes a Chromium remote-debugging port.
  - `AIWARP_PROFILE` uses a different profile folder, which lets a second instance run alongside the first.

## Releases

| Version | Highlights |
|---------|------------|
| v6.4.27 | B&W inversion and Hue rotation now cover the full app viewport, including its HUD and controls; freeze frames preserve the page filter |
| v6.4.25 | Full-canvas grayscale inversion and Hue rotation cover the captured LENS picture, initialization art, Raum showcase and visual easter eggs |
| v6.4.24 | LENS-aware B&W/Hue sliders activate paired voices with live previews; playlist playback and imports start automatic background extraction |
| v6.4.23 | Protected built-in playlist entry, visible playable playlist cards, robust XML feed parsing, duplicate-request coalescing, and direct playback fallback |
| v6.4.22 | Bottom-dock B&W/Hue tuning sliders and locks; randomized FX settings and a variable Auto Glitch roll on randomize / preset auto-cycles |
| v6.4.21 | Raum tracks now run the cover artwork as a main-canvas slideshow with exclusive audio-aware glitch sequences and a hidden signature easter egg |
| v6.4.20 | Pole · R showcase replaced by four original synthesized Raum X-R tracks (bundled, offline) with cover art, back cover and full credits |
| v6.4.19 | Softer Milkdrop subtle blends; suggested playlists stay in the audio-aware LENS and stop when another source starts |
| v6.4.18 | DROP THE MILK? adds 80 shuffled, audio-reactive Milkdrop-inspired Canvas presets, FX blending and persistent opacity lock |
| v6.4.17 | Mode-aware FX profiles for all 29 AI Warp modes, deterministic automation, and blended Ghost/V-Sync/R3/Canvas effects |
| v6.4.16 | Final: YT LENS audio now uses the direct Windows process-loopback audio driver (only this app's sound, system loopback fallback) |
| v6.4.15 | YT LENS audio is now real: the shell captures the app's own audio with the direct per-process audio driver (system-loopback fallback) and drives the audio monitor, cadence and EAR modes |
| v6.4.14 | Fixed: local-media pause button (double toggle) and YT LENS frames lost after switching to local media and back |
| v6.4.13 | Window keeps its border and stays resizable/draggable while the lens runs (no forced full screen) |
| v6.4.12 | Lens window hidden off-screen, audio monitor / cadence / EAR driven by picture motion |
| v6.4.11 | YT LENS: YouTube plays in a second AI WARP LENS window and its real pixels feed the canvas pipeline |
| v6.4.10 | YouTube overlay bridge removed; effect engine runs exactly as on imortalimp.nl/ai_warp_engine |
| v6.4.9 | Site engine, Ghost Protocol and V-Sync restored verbatim (no boosts, no added clamps); YouTube overlay moved to a separate bridge file |
| v6.4.8 | Original engine and skewing kept, plus per-mode compositor FX overlay on YouTube playback (the original SVG lens is ignored by the YouTube iframe) |
| v6.4.7 | Original AI WARP modes, skewing and YouTube path restored verbatim (all v6.4.2–v6.4.6 FX changes removed); opacity/alpha clamps kept |
| v6.4.6 | (superseded) AI WARP modes now visible on real YouTube playback (compositor overlay layers; SVG filter removed from the iframe chain) |
| v6.4.5 | Per-mode pixel FX on the YouTube feed via SVG (superseded: browsers ignore SVG filters on the iframe) |
| v6.4.4 | Warp FX boosted on the YouTube path; canvas skew smoothed (fine strips, coherent noise) |
| v6.4.3 | All master levels and voice alphas hard-clamped; Ghost, V-Sync and automation paths verified; Complete · Ready |
| v6.4.2 | All 29 AI Warp modes boosted |
| v6.4.1 | Fixed missing GC#(3) helpers (Data Bands, VHS Glitch) and `motionGlow`; screen share and mic hotkeys confirmed |
| v6.4.0 | First standalone installer |

## Licence

The software is dedicated to the public domain under [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/) and [The Unlicense](https://unlicense.org/). The imortalimp.nl site content is CC BY 4.0. See [shell/LICENSE.txt](shell/LICENSE.txt).

## Credits

River Lyle Reuveni ([imortalimp.nl](https://imortalimp.nl) · [@TheImortalimp](https://github.com/TheImortalimp)), [Arena.ai](https://arena.ai) Agent Mode, and GitHub Copilot.

The four Raum X-R tracks were created by GitHub Copilot as original synthesized works, prompted and directed by River Lyle Reuveni, and are dedicated to Stefan Betke (Pole) and the artists of the album *R* (Daniel Meteo, Burnt Friedman, Kit Clayton). Support the original: <https://pole-stefanbetke.bandcamp.com/album/r>
