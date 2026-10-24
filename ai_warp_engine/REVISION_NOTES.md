# Final Revision — AI Warp Engine / Media Page

## Delivered

- Merged the **Glitch Revision 3 Ghost Protocol v11 and V-Sync v11 Canvas systems**: DIA/SPECTRUM/CRT/PLASMA/LED ghosts; scanphasing, Color Harmony, and Sharp V-Sync; hard channel splits, Old-Soul echo, CRT/LED texture, tears, roll, hairline scan, tracking, phosphor, and color/dia light bursts.
- Connected those systems to Auto Glitch, Continual Randomization, Timed Automation, Automated Masters, Soft-Eye/Crescendo, the audio/visual ear, presets, level locks, and retained V-Sync rails.
- The R3 system implementation remains Canvas-only; no SVG filter or filter-URL pipeline was reintroduced.

- Added **17 independently named Glitch Revision 3 rails** to the existing 17 voices: `ASCII GC#(3)` through `VHS Glitch GC#(3)`.
- Kept all original voices intact; the mixer now shows **34 voices** with a clear **GLITCH REVISION 3** divider.
- Added `r3_gc3_factories.js`. Its Revision 3 factory block is copied verbatim from the supplied Glitch Revision 3 source and is run through an isolated Canvas/WebGL bridge, so its names and state cannot collide with the original 17 rails.
- Kept the R3 B&W Invert and Hue/Color Cycle rails as independent Canvas master rails. Revision 3 supplies these as master/filter-path controls rather than timed canvas factories; the implementation is Canvas-only to meet the SVG-free requirement.
- Removed all SVG filter markup, SVG filter URL usage, SVG element mutation, and SVG filter updater code from every shipped HTML/JS/CSS entry page. The application now uses Canvas/WebGL effects plus ordinary CSS color transforms for the YouTube iframe shell.
- Changed the left mixer to use measured top and bottom UI rails with `ResizeObserver`. The scroll viewport ends above the automation/preset dock, so the final voice cannot be concealed behind it.

## Files changed / added

- `ai_warp_engine.html`, `media_page.html`, `index.html`
- `ai_warp_engine.js`
- `media_page.js`
- `media_page.css`
- `r3_gc3_factories.js` (new)
- `r3_ghost_vsync.js` (new)
- `REVISION_NOTES.md` (this file)

## Automation and preset confluence update

- Dynamic machine presets now deliberately include both voice banks and rotate a core/R3 anchor so all 17 `GC#(3)` factories receive guaranteed preset coverage.
- Applying a pre-existing 17-voice preset derives restrained `GC#(3)` companion levels where it has no explicit R3 values; explicit R3 values always take precedence.
- Auto Glitch, Continual Randomization, Timed Automation, Automated Masters, board randomization, and Crescendo now route their selections through a core + `GC#(3)` confluence selector while respecting per-voice locks and user-disabled rails.
- Continual Randomization now owns and rotates a tracked paired selection; it fades only its own earlier selections and never retires manually selected voices.
- User presets now save 34 levels, locks, automation switches, Soft-Eye mode, and per-method V-Sync levels.

## YouTube Warp Bus update (v6.3)

- **Problem:** the YouTube iframe is cross-origin, so its pixels can never be read into the Canvas. In `sourceMode === 'youtube'` the glitch canvas was simply cleared, which meant the canvas-sampling voices (VHS Glitch, Data Bands, …) and the 29-mode Canvas AI Warp Slicer had nothing to act on — the YouTube picture sat mostly outside the effects pipeline, transformed only by a gentle shell skew and a partial filter mapping. The R3 factories' `ytLive`/`localVid` branches also checked `body.playback-on` / `body.local-video`, classes nothing ever toggled, so those live-feed paths were dead.
- **Warp Bus:** `updateYouTubeTransformsAndFilters` is now a full bus. Every one of the **29 AI Warp modes** maps to a distinct, audio-reactive CSS signature on `#ytWarpShell` — damped 3D skew/rotate/scale/drift plus per-mode behavior: seamless twirl spin (360°≡0°), kaleidoscope/quad-mirror flips, prism & RGB drop-shadow chroma splits, quantum jitter, active-cam jumps, zoom-blur, water-drop ripples, solarize, neon-edge echoes, and more.
- **Voice bus:** all **34 voices (17 core + 17 `GC#(3)`)** now contribute to the live video via a cumulative CSS filter chain — hue cycles (HSL Rainbow / Hue Cycle), invert (B&W Invert), exponential-decay flashes and 6–15 Hz strobe, neon/gemini chroma-split drop-shadows, VHS tracking jitter and luma shimmer, Data Bands clip-path tear windows, Data Bomb hits, and scanline contrast coupling. Per-voice SIZE/DEPTH sliders shape the video treatment (alpha-weighted across core + GC#(3) rails). Self-drawing overlay voices (ASCII, Matrix, glyphs, bombs, scanline rasters) keep painting onto the transparent glitch canvas stacked above the shell.
- **Loop order:** in YouTube mode the Canvas AI Warp Slicer step is skipped (the bus owns the video; slicing a transparent canvas only smeared overlay voices). Overlay voices, Ghost Protocol and V-Sync still render in place above the feed.
- **Body classes:** `media_page.js` now toggles `playback-on`, `local-video` and `youtube-feed` from every media-kind transition (YouTube, local video/audio, Pole·R, synth, stop), activating the previously dead R3 `ytLive` branches (film grain and roll bars keyed to live playback).
- **Cleanup:** leaving YouTube mode resets transform/filter/clip-path on `#ytWarpShell`; `#ytWarpShell` gained `will-change`/origin/backface hints; dead `Bloom`/`Solarize`/`TapeStop` voice references removed from the YouTube path; added a one-time HUD toast when the bus first engages.
- **Homepage showcase:** the imortalimp.nl `index.html` / `home.html` AI WARP ENGINE spotlight panel now documents the Warp Bus (10-line release spec, README pane), ships chips for every shipped file (including the `index.html` alias and the showcase fragment), and points its download buttons at `ai_warp_engine_r3_canvas_v6.3.zip`. The standalone `ai_warp_showcase_section.html` fragment is a byte-exact copy of that homepage section.
- **Files changed:** `ai_warp_engine.js`, `media_page.js`, `media_page.css`, `index.html` / `ai_warp_engine.html` / `media_page.html` (meta description), `README.md`, `REVISION_NOTES.md`, `media_page.json`, homepage `index.html` / `home.html`, `ai_warp_showcase_section.html`.

## SVG Warp Lens update (v6.3.1)

- **Request:** "render the YouTube pixels through the SVG." This re-introduces SVG filtering — narrowly scoped — after the R3 revision had deliberately removed all SVG filter usage. It overrides that earlier SVG-free requirement per user request.
- **How it works:** an inline `<svg>` graph (`#ytSvgWarp` = `feTurbulence` → `feDisplacementMap`, R/G channel selectors, 140% overflow region) is referenced via CSS `filter: url(#ytSvgWarp)` on `#ytWarpShell`, prepended before the existing cumulative CSS color chain. The browser compositor rasterizes the **actual live YouTube pixels** through per-pixel displacement — true melting, tearing, and liquefaction that CSS transforms cannot express.
- **Cross-origin stays safe:** this is display-only. JS writes only `baseFrequency`, `scale`, and `type` attributes on the two primitives; the pixels are never read back into script (so freeze-frame PNG export of the YouTube feed remains impossible — unchanged browser limitation).
- **Per-mode character:** each of the 29 warp modes gets its own turbulence field (e.g. `analog_crt` = 0.9/0.001 horizontal smear, `quantum`/`earthquake`/`cybergrid` = sharp `turbulence` type, `liquid` = slow low-frequency `fractalNoise`), amplitude is voice-boosted (VHS +70%, Data Bands +90%, Strobe, Data Bomb), bass-swells, and hard-light gains +20%. Zeroed whenever warp strength is 0, the lens (`aiWarp.setSvgWarpActive()`), or YouTube mode is exited — no perf cost when idle.
- **Files changed:** `ai_warp_engine.js` (lens state, `resolveSvgWarpLens()` mode table, bus integration, teardown, toggle API), the three HTML aliases (inline graph + meta), `media_page.css` (`.svg-warp-defs` host — deliberately NOT `display:none`, which would kill `url()` refs), `README.md`, `REVISION_NOTES.md`, `media_page.json`, homepage `index.html` / `home.html` docs, `ai_warp_showcase_section.html` (re-synced).

## 📡 Tab Capture update (v6.4)

- **Feature:** `getDisplayMedia`-powered Tab Capture. The user picks any browser tab (e.g. YouTube / YouTube Music) with optional "tab audio"; real decoded pixels play through `#localVideo`, so the **entire Canvas pipeline** — all 34 voices with genuine frame sampling, the 29-mode Warp Slicer (`renderAiWarp`), R3 Ghost/V-Sync and the R3 factories' live-feed branches — acts on the true picture; tab audio connects `createMediaStreamSource → captureGain → masterAnalyser` (**analysis only**, never routed to `destination`), replacing the synthetic ~130 BPM YouTube cadence with genuinely audio-reactive drive.
- **Why this path:** there is no YouTube API (free or paid) that exposes media streams for processing, and YouTube Developer Policies forbid separating/modifying audio or video components of delivered content. User-initiated tab capture is the only legitimate browser-native route — no download, no extraction, nothing stored.
- **Integration details:** captures into the existing `#localVideo` element (muted during capture so audio is never double-played); `window.video = localVideo` now exposes the element as the bare global the verbatim R3 factory block reads, making its `localVid` frame-sampling branches live for both local files and capture; `syncPlaybackClasses` gains the `capture` kind (`local-video` set since real frames are on canvas); playback-stop is wired into `pauseAllOtherMedia`, the browser "Stop sharing" `track.ended` hook, the Space transport (pause/resume), and the 📡 button toggle.
- **UI:** new `📡 CAPTURE TAB` button in the YouTube cluster (auto-disabled when `getDisplayMedia` is unsupported), `capture-active` green styling, monitor badge `📡 TAB CAPTURE [ACTIVE]`, HUD toasts for live/declined/visual-only states, and hotkey `N` (added to the shortcuts overlay).
- **Fallbacks:** fine-grained audio constraint rejection falls back to `{ video: true, audio: true }`; picker cancel/denial shows a HUD notice; visual-only shares (no audio track) notify the user to re-share with tab audio.
- **Files changed:** `media_page.js`, the three HTML aliases (button + shortcut row + meta), `media_page.css`, `ai_warp_engine.js` (header), `README.md`, `REVISION_NOTES.md`, `media_page.json`, homepage `index.html` / `home.html` docs, `ai_warp_showcase_section.html` (re-synced).

## Capture modes and hotkeys follow-up

- **Display capture:** the `getDisplayMedia` picker now supports browser tabs, application windows, and full-screen sharing through the same Canvas pipeline. The microphone remains an independent audio input, so it can be used alongside shared display audio; neither input is routed back to speakers.
- **YouTube continuity:** the existing YouTube iframe/Canvas Warp Bus and Ear modes remain available. Display capture temporarily takes visual focus and restores the YouTube feed when sharing ends. Canceling the browser picker leaves the current media source uninterrupted.
- **Hotkeys:** `M` toggles microphone input, `Y` loads the featured YouTube playlist, `E` cycles Ear modes (`AUTO` / `AUDIO` / `CANVAS`), and `N` toggles browser-tab/window/full-screen capture.
