# AI WARP ENGINE 6.4.38.1 COMPLETE (ALL WINDOWS x64 NATIVE) -noffmpeg_(~Gyan-D)

Browser visual synthesizer with 34 glitch voices, 29 warp effects, local media,
YouTube playback, audio analysis, presets and Canvas screenshots.

Warp effects use synchronized completed frames, continuous timing, 240 ms mode
blending and last-good-frame recovery. Three.js is bundled locally; the main
app's Content Security Policy blocks remote scripts. The app blocks new-window
and external-browser links without filtering accessibility input.

Freeze Frame now includes the completed engine canvas, fullscreen CRT scanlines,
vignette and final page filters without HUD. Browser output rasterizes fullscreen
effects before applying page filters once. Windows uses full compositor capture,
including underlying LENS/media and display-only treatments. Capture restores
controls after success/failure/timeout. Offscreen scratch buffers already feed
the completed canvas; they are not duplicated into the export.

Local playback requires browser-supported codecs. AVI and other unsupported
files must be converted locally to H.264/AAC MP4 or AAC M4A before loading.
Automatic Windows conversion uses installed OS codecs/MediaTranscoder, with
explicit errors for unsupported inputs. No FFmpeg or Windows codec pack is bundled.
Desktop YouTube LENS capture is Windows-only.
For readable YouTube pixels in a browser, use the app's screen/tab capture.

Upload this entire folder, including `audio`, under `ai_warp_engine` beside the
website's `home.html` and `index.html`. Serve over HTTPS, not `file://`.
Open [the browser app](index.html) in the same tab.

The v6.4.38.1 GitHub release provides the current app-only browser ZIP, excluding the
website-root `home.html` and `index.html` but retaining this folder's app aliases.
Windows 6.4.38.1 bundles .NET and requires installed WebView2 and Windows media
components (Windows 10 build 19041+/11 x64). Windows N needs the Media Feature Pack.
The native label does not promise every codec or an all-native UI.
Older FFmpeg-based installers remain pending source verification.

App code: CC0 / Unlicense. Bundled Three.js retains its
[MIT license](THREE-LICENSE.txt); other media retain their own rights.
