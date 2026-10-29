# AI WARP ENGINE 6.4.24 — LENS FX controls and automatic playlist extraction

Release 6.4.24 fixes B&W inversion and Hue/Color Cycle tuning on the captured YouTube LENS Canvas and automates playlist extraction during common playback and import flows.

## Highlights

- **LENS-visible B&W tuning:** adjusting B&W inversion strength activates the core and GC#(3) B&W voices, so the selected level affects the live LENS Canvas.
- **LENS-visible Hue tuning:** adjusting the Hue spectrum activates both Hue/Color Cycle voices; a spectrum preview marks the selected offset.
- **Live control feedback:** monochrome and hue-spectrum previews update with their sliders, alongside numeric values.
- **Automatic playlist extraction:** playing a saved playlist or importing playlist records starts background extraction automatically. Pasting/loading a playlist URL continues to play it directly while extraction runs.
- **Retained v6.4.23 protections:** the featured playlist stays pinned; XML parsing is namespace-aware, duplicate in-flight requests coalesce, and feed failures leave playlists visible and playable.

YouTube’s public playlist feed may expose only a recent subset of a playlist. Use **PLAY** to browse the full playlist directly on YouTube. Network or CORS restrictions can prevent local extraction; the saved playlist remains available for direct playback.

## Install

Run `AI-WARP-ENGINE-6.4.24-x64-setup.exe` on Windows 10/11 x64. It installs per-user without administrator access.

**Installer SHA-256:** `94CCD2A3F3587DA3C72E057A8275AF685D5D1570279852759D5F27B093ADBBA8`

[Download the AI WARP ENGINE User Manual](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/download/v6.4.24/MANUAL.md)
