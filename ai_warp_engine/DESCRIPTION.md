# AI WARP ENGINE 6.4.37 - Glitch Hardened & FX Fixed

Browser visual synthesizer with 34 glitch voices, 29 warp effects, local media,
YouTube playback, audio analysis, presets and Canvas screenshots.

Warp effects use synchronized completed frames, continuous timing, 240 ms mode
blending and last-good-frame recovery. Three.js is bundled locally; the main
app's Content Security Policy blocks remote scripts. The app blocks new-window
and external-browser links without filtering accessibility input.

Local playback requires browser-supported codecs. AVI and other unsupported
files must be converted locally to H.264/AAC MP4 or AAC M4A before loading.
Automatic FFmpeg conversion and desktop YouTube LENS capture are Windows-only.
For readable YouTube pixels in a browser, use the app's screen/tab capture.

Upload this entire folder, including `audio`, under `ai_warp_engine` beside the
website's `home.html` and `index.html`. Serve over HTTPS, not `file://`.
Open [the browser app](index.html) in the same tab.

The v6.4.37 GitHub release provides an app-only browser ZIP, excluding the
website-root `home.html` and `index.html` but retaining this folder's app aliases.
The Windows v6.4.37 installer is not published; website download links identify
the previous published 6.4.36 installer.

App code: CC0 / Unlicense. Bundled Three.js retains its
[MIT license](THREE-LICENSE.txt); other media retain their own rights.
