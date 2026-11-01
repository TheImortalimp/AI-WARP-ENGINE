# AI WARP ENGINE 6.4.38.21 ULTIMATE

Browser visual synthesizer with 34 glitch voices, 29 warp effects, local media,
YouTube playback, audio analysis, presets and Canvas screenshots.

The effect banks (`glitch_engine.js`, `r3_gc3_factories.js`, `r3_ghost_vsync.js`
and `yt_lens.html`) are byte-identical to 6.4.36 and pinned by SHA-256. The engine
checks its frozen effect registry at startup, and `attestEffectBanks()` re-hashes
the files the page was actually served.

Warp effects use synchronized completed frames, continuous timing, 240 ms mode
blending and last-good-frame recovery. The lens canvas, every effect buffer, the
Tech Artist WebGL buffer and the fullscreen layers share one size at any window
size or display scaling, and a resize redraws the last picture instead of blanking
the lens. If WebGL cannot start, the 2D fallback runs and WebGL is retried at most
every 5 seconds. Three.js is bundled locally; the main app's Content Security
Policy blocks remote scripts. The app blocks new-window and external-browser links
without filtering accessibility input.

Freeze Frame includes the completed engine canvas, fullscreen CRT scanlines,
vignette and final page filters, and never the HUD. The HUD (bars, mixer, badges,
the HIDE U.I. tab, drawers and dialogs, including descendants and anything
mid-transition) is hidden for the capture and restored after success, failure or
timeout. Browser output rasterizes fullscreen effects before applying page filters
once. Windows uses full compositor capture, including underlying LENS/media and
display-only treatments. Offscreen scratch buffers already feed the completed
canvas; they are not duplicated into the export.

Local playback requires browser-supported codecs. AVI and other unsupported
files must be converted locally to H.264/AAC MP4 or AAC M4A before loading.
The Windows app converts them with installed OS codecs through MediaTranscoder
and reports an explicit error for inputs Windows cannot decode. No codec pack is
bundled. Desktop YouTube LENS capture is Windows-only.
For readable YouTube pixels in a browser, use the app's screen/tab capture.

Upload this entire folder, including `audio`, under `ai_warp_engine` beside the
website's `home.html` and `index.html`. Serve over HTTPS, not `file://`.
Open [the browser app](index.html) in the same tab.

The v6.4.38.21 GitHub release provides the current app-only browser ZIP, excluding
the website-root `home.html` and `index.html` but retaining this folder's app
aliases. Windows 6.4.38.21 bundles .NET and requires installed WebView2 and
Windows media components (Windows 10 build 19041+/11 x64). Windows N needs the
Media Feature Pack. The native label does not promise every codec or an
all-native UI. The earlier 6.4.37 and 6.4.37.1 Windows installers remain withheld
pending source review.

App code: CC0 / Unlicense. Bundled Three.js retains its
[MIT license](THREE-LICENSE.txt); other media retain their own rights.
