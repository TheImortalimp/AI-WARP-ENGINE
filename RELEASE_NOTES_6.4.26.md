# AI WARP ENGINE 6.4.26 — Full app-frame B&W and Hue FX

Release 6.4.26 extends the existing B&W inversion and Hue/Color Cycle from the rendered LENS canvas to the complete app viewport.

## Highlights

- **Whole-page FX:** Adjustable grayscale/inversion and hue rotation now apply after page composition, including the rendered LENS picture, full HUD, controls, dialogs, overlays, and artwork.
- **App-matched browser and Windows editions:** Both editions use the same viewport-level compositor in the shared app files. The Windows WebView2 host displays the filtered app frame; native Windows window chrome remains outside the web content.
- **Preserved Canvas effects:** Hard Light remains scoped to the effects canvas instead of tinting the interface.
- **Freeze-frame parity:** Page screenshots apply the same viewport B&W/Hue filters. The canvas-only fallback applies the page filter together with any canvas filter.
- **Retained v6.4.25 behavior:** Full-canvas LENS inversion, Hue spectrum, and existing playlist extraction improvements remain included.

The existing bottom-dock B&W and Hue controls, locks, randomized rolls, and automation continue to determine when and how strongly the full-frame effects are applied.

## Install

Run `AI-WARP-ENGINE-6.4.26-x64-setup.exe` on Windows 10/11 x64. The installer is per-user and does not require administrator access.

[Download the AI WARP ENGINE User Manual](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/download/v6.4.26/MANUAL.md)

## Windows installer checksum

SHA-256: `5E97051884CA9EFAAEA6F0E4C10F91317B796BD08F577F9E1D5A0DB24C0E5281`
