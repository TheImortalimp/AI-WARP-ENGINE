# AI WARP ENGINE 6.4.37.1 - freeze-frame fix

The browser patch is public in the v6.4.37 release as
`AI-WARP-ENGINE-6.4.37.1-browser.zip`. It contains only `ai_warp_engine/`,
excluding website-root home/index pages, native binaries and FFmpeg.
The Windows patch is private for GyanD; not a public Windows release. The 6.4.37
public installer remains gated on verified FFmpeg corresponding-source coverage.

Freeze Frame (button or S) now captures the Windows WebView2 composited viewport:
LENS/local media, completed voice/Warp/Ghost/V-Sync/Milkdrop canvas output,
fullscreen CRT scanlines, vignette and final page filters. HUD, controls,
notifications and the decorative OSD badge are hidden during capture and restored
after success, failure or timeout. Repeated requests are rejected while busy.
Capture does not stop media playback or flatten intentional glitch rhythms.

Browsers export the final FX canvas plus rasterized fullscreen CRT/vignette
layers, then apply the page filter once. Cross-origin YouTube iframe capture is
not possible from browser Canvas: an explicit message requests tab/screen capture.
Screenshot errors are reported rather than exporting an incomplete success.

All offscreen engine scratch buffers already compose into the final canvas;
adding them again would duplicate FX. Windows compositor capture includes actual
display-only CSS/media treatments instead.

The patch has app/EXE/installer version 6.4.37.1 and new asset cache revisions.
Use the complete installer; the EXE alone is not a portable app. The installer
remains unsigned and elevated and applies Windows cursor/theme/sound policies.
FFmpeg is unchanged; its pending source/materials review still applies.

The approved 6.4.37 publication workflow still stages the original installer.
This private patch does not silently replace that workflow's pinned binary hash.

Validation: 29 focused JavaScript tests passed. The actual Windows host was
tested with pixel checks: native and browser output include the vignette and
CRT treatment, page inversion applies once, underlying media layers remain
visible, and the HUD mask restores. Native output measured 1338 x 804 pixels;
native/browser center and edge colors agreed within one channel value.
