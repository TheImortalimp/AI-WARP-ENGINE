# AI WARP ENGINE — User Manual

**Applies to:** AI WARP ENGINE standalone Windows app, release 6.4.27
**Platform:** Windows 10/11 x64

AI WARP ENGINE is a real-time visual synthesizer. Choose a visual or media source, then layer AI Warp modes, glitch voices, Ghost Protocol, V-Sync and audio-reactive automation over it. Release 6.4.27 makes locks hard and the screenshot true: voice locks and new Speed, Opacity, Ghost and V-Sync locks in the bottom controls are enforced every frame against Automated Masters, Continual Randomization, Timed Auto and presets; Freeze Frame (S) saves the full LENS canvas as an opaque PNG without the HUD or app frame, and the canvas no longer shrinks when zooming out (YouTube iframe pixels cannot be read by browsers - use local media, YT LENS or tab capture); Neon Gradient (core and GC#(3)) is a much stronger rotating ribbon with bloom and wash; GC#(3) ASCII streams a continual luminance-mapped character sheet; Twirl is slower and smoother. Release 6.4.26 extends adjustable B&W inversion and Hue rotation across the complete app viewport, including the HUD, controls, dialogs and overlays, and preserves those treatments in freeze-frame screenshots. Release 6.4.25 applies them to the complete LENS composition, including captured frames, initialization art, the Raum X-R showcase and visual easter eggs. Release 6.4.24 makes bottom-dock B&W/Hue tuning activate the paired voices on the LENS Canvas and starts playlist extraction automatically during playback/import. Release 6.4.23 improves YouTube playlist visibility and feed extraction. Release 6.4.22 adds locked B&W/Hue fine-tuning and randomized automation rolls. Release 6.4.21 runs the Raum cover artwork as a slideshow with exclusive glitch sequences while those tracks play. Release 6.4.20 adds the bundled RAUM · X-R showcase of four original tracks. Release 6.4.19 added the subtle-blend DROP THE MILK? feedback effect and audio-aware YouTube playlist playback in the standalone YT LENS.

## Contents

1. [Install and launch](#install-and-launch)
2. [A first session](#a-first-session)
3. [Choosing a source](#choosing-a-source)
4. [YouTube and the standalone YT LENS](#youtube-and-the-standalone-yt-lens)
5. [Audio reactivity](#audio-reactivity)
6. [Visual effects](#visual-effects)
7. [Automation and presets](#automation-and-presets)
8. [Keyboard shortcuts](#keyboard-shortcuts)
9. [Settings, privacy and performance](#settings-privacy-and-performance)
10. [Troubleshooting](#troubleshooting)

## Install and launch

1. Download and run `AI-WARP-ENGINE-6.4.27-x64-setup.exe` from the [v6.4.27 GitHub release](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/tag/v6.4.27).
2. Run the installer and accept the licence terms. It installs for the current user and does not require administrator access.
3. Launch **AI WARP ENGINE** from the desktop shortcut or Start Menu.

The release is a self-contained Windows x64 app. It uses the Microsoft Edge WebView2 runtime; current Windows installations generally include the runtime. The application and web profile are stored under `%LOCALAPPDATA%\AI-Warp-Engine`; user settings are kept with the WebView2 profile.

The main window remains a normal resizable Windows window. Use **F11** to toggle its fullscreen state. The YouTube LENS is a separate supporting window managed by the app; keep the app running while lens playback is active.

## A first session

1. Launch the app and choose a source: leave the generated visual running, open a local file, play a synth groove, or load a YouTube video/playlist.
2. Cycle an **AI WARP** mode with `W`, and tune the warp strength and master opacity with their sliders.
3. Enable one or more rails in the **FX Mixer**. Each rail has its own on/off and level controls; the original 17 voices and 17 `GC#(3)` voices are independent.
4. Try **GHOST** or **V-SYNC**, or start **Auto Glitch** for changing accents.
5. For Milkdrop-inspired feedback, enable **DROP THE MILK?** or press `D`. Adjust **MILKDROP opacity** and use its lock control if you want to keep your chosen blend through preset changes.
6. Press `?` at any time to open the in-app shortcut reference.

Effects can be combined. If a result becomes too intense, lower the master opacity/warp strength, reduce individual mixer rails, unlock or lower Milkdrop opacity, or press `Z` to clear the effect rails.

## Choosing a source

### Generated visuals

The app can run its built-in generative Canvas visuals without opening a media file. This is a useful starting point for exploring the effect controls and presets.

### Local media

Use the app’s media/file controls to load local images, video or audio. Video is rendered into the visual pipeline; audio is monitored by the analyser and can accompany artwork. Playback controls include play/pause and transport where applicable. Selecting another source stops the current media first.

Common formats include image files and media containers supported by the installed WebView2 codecs. Video codec support depends on the file’s encoding as well as its extension.

### RAUM · X-R showcase (bundled tracks)

Click **RAUM · X-R (4)** in the toolbar to open the showcase drawer. It lists four original, fully synthesized tracks, each with cover art, a length and a **PLAY** button, followed by the back cover and the thanks and dedication panel.

| # | Title | Length |
|---|---|---|
| 1 | pole - Я - raum 1 - variant X-R | 7:15 |
| 2 | pole - Я - raum 2 - variant X-R | 8:05 |
| 3 | pole - Я - raum 3 - variant X-R | 9:03 |
| 4 | pole - Я - raum 4 - variant X-R | 9:03 |

- Tracks 1 and 2 are jungle/breakcore with tempo drops, beat silences and crescendos. Tracks 3 and 4 are ambient, with harmonic beats and glitches under glass-bell drones.
- The files ship inside the app, so no internet connection is needed. The audio feeds the analyser, so the visual effects react to it.
- Starting one track pauses the others; starting any other source (YouTube, local media, synth, capture) stops the showcase, and the reverse is also true.
- **Credits:** created by GitHub Copilot (AI), prompted and directed by River Lyle Reuveni. Dedicated to Stefan Betke (Pole) and the artists of the 1998 album *R* on DIN: Daniel Meteo, Burnt Friedman and Kit Clayton. The tracks contain no samples or recordings by them and are not affiliated with them. Support the original: <https://pole-stefanbetke.bandcamp.com/album/r>

### RAUM · X-R artwork slideshow and glitch sequences

**Artwork slideshow and exclusive glitch sequences (v6.4.21):** while a RAUM · X-R track plays, the main canvas switches to a slideshow of the four covers and the back cover, replacing the init artwork or any other source, and is warped by original glitch sequences that follow the track clock and its audio: TEAR, GRID, MIRROR (backwards-Я fold), SMEAR (datamosh trail), COLUMNS and RIPPLE (glass-bell halos, ambient tracks), plus a decoding caption, a giant reversed Я that surfaces during beat silence, a reversal flash when the beat re-enters, and a hidden SIGNATURE lattice every 64 seconds of track time. Pausing or starting any other source restores the previous visuals.

### Microphone and screen capture

- Press `M` or use the MIC control to toggle microphone input for analysis.
- In a standard browser build, press `N` to start display capture, then select the tab, window or screen in the browser’s capture picker. Stop sharing with the browser’s sharing UI or `N`.
- The standalone Windows app routes its built-in YouTube playback through YT LENS instead of requiring the browser capture picker.

Capture audio is routed to the analyser, not mixed back to the output by the capture analyser connection. Enable shared/tab audio in the capture picker if you want the captured source’s sound to drive audio-reactive effects.

## YouTube and the standalone YT LENS

### Load a video or playlist

- Enter a YouTube video URL, playlist URL or supported ID in the YouTube input and choose **LOAD** (or press Enter in the input).
- Use **PLAY FEATURED PLAYLIST** or press `Y` for the featured playlist.
- Use the YouTube **RANDOM**, **PREV** and **NEXT** controls to navigate the knowledgebase/history. The Next control can offer a random item when there is no later history entry.
- The knowledgebase controls can add IDs, search/filter saved entries, import/export the list, and extract playlist entries when available.

In the standalone app, suggested and featured playlist-only playback is opened in the hidden supporting **AI WARP LENS** window. Its frames feed the app’s Canvas pipeline, and the lens audio is tapped by the desktop host for the audio analyser. This lets the usual warp, voice and audio-reactive path operate on the lens source. The LENS window is intentionally kept out of the way; do not close it manually during playback.

When another source starts, the app closes the active LENS playback and ends its audio/frame capture. If a new source starts while a lens is still opening, the pending launch is cancelled so it cannot unexpectedly take over later. Video selections that cannot use the lens may use the in-page player fallback; playlist-only suggestions require the LENS in the standalone app so they do not play outside the visual path.

The browser-only build does not have the Windows LENS host. It uses its in-page YouTube player. YouTube playback requires network access and can be affected by content availability, age/region restrictions, embed restrictions and browser connectivity.

## Audio reactivity

The analyser responds to available source audio, microphone input, captured/shared audio, or the built-in synth. Bass, midrange, treble and beat/drive signals feed the visual system. The audio monitor badge indicates the current monitor source when available.

- **MIC** (`M`) toggles microphone analysis. Grant microphone permission when prompted.
- **SYNTH DUB** starts or cycles the built-in procedural synth grooves. The groove is generated locally and can drive the effects without a media file.
- **Audio Reactive Ear** (`E`) cycles **AUTO**, **AUDIO** and **CANVAS** response modes. AUTO follows the available signal; AUDIO prioritizes the analyser input; CANVAS responds to picture motion.
- The standalone YT LENS audio monitor uses the Windows host’s direct per-process audio tap where possible, with a system-loopback fallback. Keep the app and lens playback active for its signal to be available.

If the monitor is idle, check that the selected media is playing, that the microphone or shared audio permission is enabled as appropriate, and that the selected Ear mode matches the signal you want to use.

## Visual effects

### AI Warp

Cycle modes with `W` or use the warp-mode control. There are 29 modes:

- **Algorithmic:** liquid, prism, breathe, fold, aurora, vortex, cybergrid, hyperdrive, kaleidoscope, quantum, ripple and analog CRT.
- **NewBlue-inspired:** roll, active cam, shear, wiggle, zoom blur, video tile, twirl, light ring, earthquake, water drop, RGB shift, quad mirror, fish eye, pinch, solarize, cathode beam and neon edges.

Use the AI Warp strength slider to scale the warp and master opacity to control the overall visual blend. Mode-aware FX profiles coordinate selected Ghost, V-Sync, Canvas and automation responses with the active mode.

### FX Mixer

The mixer has 34 independent voices: 17 core effects and their 17 `GC#(3)` Revision 3 counterparts.

| Core voice | R3 counterpart |
|---|---|
| Neon Grad | Neon Grad GC#(3) |
| Scanlines | Scanlines GC#(3) |
| Data Bands | Data Bands GC#(3) |
| Flash | Flash GC#(3) |
| HSL Rainbow | HSL Rainbow GC#(3) |
| ASCII | ASCII GC#(3) |
| Font Burn | Font Burn GC#(3) |
| Strobe | Strobe GC#(3) |
| Gemini Bomb | Gemini Bomb GC#(3) |
| Tech Artist | Tech Artist GC#(3) |
| VHS Glitch | VHS Glitch GC#(3) |
| Data Bomb | Data Bomb GC#(3) |
| Ink Glyphs | Ink Glyphs GC#(3) |
| Matrix ASCII | Matrix ASCII GC#(3) |
| Matrix Chord | Matrix Chord GC#(3) |
| B&W Invert | B&W Invert GC#(3) |
| Hue/Color Cycle | Hue/Color Cycle GC#(3) |

Toggle rails individually and adjust their level/size/depth controls where provided. Use `1`–`9` to toggle the first nine mixer rails. Background automation respects protected/locked voice intent.

### Ghost Protocol

Use **GHOST** or `G` to cycle **OFF → DIA → SPECTRUM → CRT → PLASMA → LED**. The level control adjusts the selected mode; the Ghost Null control clears its mode and level. OFF bypasses the effect.

### V-Sync

Use **V-SYNC** or `V` to cycle **OFF → SCANPHASING → COLOR HARMONY → SHARP**. Adjust the level for the selected rail. The lock protects the V-Sync selection from changes; the V-Sync Null control clears its retained value.

### DROP THE MILK? — subtle blends

Enable the toolbar control or press `D` to layer Milkdrop-inspired feedback over the current visual. The effect includes 80 shuffled presets, feedback zoom/rotation, analyser-reactive polar waveforms and colour blooms. In 6.4.19, opacity and blend strengths are intentionally more subtle.

- **Unlocked opacity:** follows the active preset’s opacity automation.
- **Locked opacity:** holds the manually selected level as presets change.
- Opacity and lock preference persist across reloads in the app’s local profile.

### Other visual controls

- **Soft-Eye** control cycles OFF, CALM, SURGE and CRESCENDO pacing states.
- **Crescendo** (`C`) triggers a temporary coordinated burst.
- **Freeze frame** (`S`) exports a PNG still of the current visual.
- **Zero all** (`Z`) clears effect rails/levels when you want to reset the mix without changing the application version.

## Automation and presets

Automation can change effects over time. Try one system at a time at first; use the corresponding control to stop it before building a different look.

- **Auto Glitch** (`A`) schedules changing glitch accents and synchronized effects.
- **Continual Random** changes tracked effect combinations over time.
- **Timed Automation** (`T`) cycles timed effect/cadence behavior.
- **Auto Masters** retargets eligible master rails.
- **Randomize** rolls B&W/Hue settings unless locked; Auto Glitch also receives a fresh 50/50 on/off roll. The preset auto-cycler rerolls the same FX settings and Auto Glitch state for each preset.
- **Soft-Eye** shapes intensity and pacing; it has its own control and is separate from the Audio Reactive Ear (`E`).
- **Crescendo** (`C`) creates a short, stronger burst.

The preset browser provides machine presets and saved user presets. Load a preset to restore a coordinated mix; save a user preset after dialing in a look you want to reuse. Preset and automation behavior may update unlocked controls, while the app’s protected/manual controls retain user intent. The Milkdrop opacity lock is separate and persists with its own saved preference.

## B&W and Hue fine-tuning

**Locks (v6.4.27):** every voice row has a lock, and the bottom dock has lock buttons for **SPEED**, **OPACITY**, **GHOST** and **V-SYNC**. A locked item is restored every frame, so Automated Masters, Continual Randomization, Timed Auto and presets cannot change it; moving a locked slider yourself sets the new held value. **Freeze Frame** exports the full LENS canvas without HUD.

The bottom control dock has separate **B&W INVERT** strength and **HUE SPECTRUM** sliders with live previews. Adjusting either slider activates its matching core and GC#(3) voices. In v6.4.26 the final adjustable grayscale/inversion and Hue rotation are applied to the entire app viewport after composition, so the picture, HUD, controls, dialogs and overlays are all included. Hard Light remains limited to the effects canvas; freeze-frame screenshots retain the full-page B&W/Hue treatment. The final pass includes captured LENS frames, initialization artwork, Raum X-R slideshow artwork, captions and the reversed-Я easter egg. Lock either setting to protect it from randomization and preset automation. **ROLL FX** randomizes only unlocked settings; board randomization and each preset auto-cycle also reroll unlocked settings and independently choose Auto Glitch ON or OFF with equal probability. Values and lock preferences persist on this device. The core voices and their GC#(3) counterparts remain independently controllable; automation chooses them as complementary pairs with subtle rendering differences.

## YouTube playlists and extraction

The **Knowledgebase & Playlist Vault** keeps the featured playlist as a protected built-in entry, even after resetting or clearing custom links. Saved playlist records appear above video links with **PLAY** and **EXTRACT** actions. **PLAY** opens the full playlist directly on YouTube and automatically starts video extraction in the background. Importing playlists also starts extraction automatically. **EXTRACT** can be used to retry or refresh manually. YouTube’s public feed may expose only a recent subset, and network/CORS restrictions can temporarily prevent extraction; in that case the saved playlist remains visible and playable. Pasting a playlist URL starts playback immediately while feed extraction runs in the background.

## Keyboard shortcuts

Shortcuts are ignored while typing in a text field.

| Key | Action |
|---|---|
| `Space` | Play/pause the active local media or synth when applicable |
| `W` | Cycle AI Warp mode |
| `G` | Cycle Ghost Protocol |
| `V` | Cycle V-Sync |
| `A` | Toggle Auto Glitch |
| `R` | Randomize the effects board |
| `T` | Cycle Timed Automation |
| `C` | Trigger Crescendo |
| `D` | Toggle DROP THE MILK? |
| `Z` | Zero/reset effect rails |
| `M` | Toggle microphone input |
| `N` | Toggle browser display capture (browser build) |
| `Y` | Load the featured YouTube playlist |
| `E` | Cycle Audio Reactive Ear |
| `H` | Hide/show the interface |
| `S` | Export a frozen PNG frame |
| `1`–`9` | Toggle the first nine mixer rails |
| `?` | Open/close the shortcut dialog |
| `F11` | Toggle the Windows app window’s fullscreen state |
| `F5` | Reload the app page |

## Settings, privacy and performance

- Microphone and capture access are used only when you enable those features. Windows/WebView2 can ask for permission.
- The app profile and persisted settings are stored in `%LOCALAPPDATA%\AI-Warp-Engine`. Milkdrop opacity/lock and user preset data are saved in the web profile.
- Local media is opened for playback in the app. YouTube requires an internet connection and is subject to YouTube’s playback/embed availability.
- The LENS is a separate WebView2 window used to play/capture YouTube frames and tap audio. It is managed by the host; switching sources stops it.
- High resolution, multiple simultaneous effects, screen capture and many active mixer rails increase GPU/CPU load. Reduce master opacity, warp strength or active rails if playback becomes choppy.
- `F5` reloads the app page. `F11` returns between fullscreen and the normal resizable window.

## Troubleshooting

### YouTube video or playlist does not start

Check the network connection and confirm that the video/playlist is available for embedding and playback in your region/account. Try loading a single video URL. Keep the main app open while the LENS runs. If another source starts, the lens is intentionally stopped; select the playlist again to resume it.

### Audio-reactive controls do not respond

Check the audio monitor badge and confirm the source is playing. For microphone input, enable **MIC** and allow access. For shared browser capture, include audio in the capture picker. For YouTube in the standalone app, allow a moment for LENS audio monitoring to initialize. Try **Ear: AUTO** or **AUDIO**.

### Capture or microphone permission is denied

Enable the requested permission in the Windows/WebView2 prompt or system privacy settings, then reload with `F5` and retry. Browser display capture is available in the browser build; the standalone app uses its own LENS workflow for YouTube.

### Local video has sound but no picture, or cannot play

Some containers use codecs not available to WebView2. Try a broadly supported H.264/AAC MP4. A different filename extension does not change the media’s encoded format.

### Visuals stutter

Close other GPU-heavy apps, reduce the number of active mixer rails, lower warp strength and master opacity, and avoid unnecessary high-resolution capture. The LENS frame pipeline and large canvases also consume system resources.

### Reset a visual mix

Press `Z` to clear/reset effect levels, then re-enable only the modes and voices you want. This does not uninstall the app or change the release.

## Release and project files

- Current standalone release: AI WARP ENGINE 6.4.27
- [README and project overview](README.md)
- [Short project description](DESCRIPTION.md)
- [6.4.27 release notes](RELEASE_NOTES_6.4.27.md)
- [6.4.24 release notes and installer checksum](RELEASE_NOTES_6.4.24.md)
- [6.4.23 release notes and installer checksum](RELEASE_NOTES_6.4.23.md)
- [6.4.22 release notes and installer checksum](RELEASE_NOTES_6.4.22.md)
- [6.4.21 release notes and installer checksum](RELEASE_NOTES_6.4.21.md)
- [6.4.20 release notes](RELEASE_NOTES_6.4.20.md)
- The app’s HTML, CSS and JavaScript sources are in `ai_warp_engine/`; the Windows WebView2 host and installer sources are in `shell/`.
