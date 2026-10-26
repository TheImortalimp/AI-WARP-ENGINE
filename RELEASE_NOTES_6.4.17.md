# AI WARP ENGINE 6.4.17 — Mode-aware FX

AI WARP ENGINE 6.4.17 adds coordinated, deterministic FX blending for all 29 AI Warp modes.

## Highlights

- Each warp mode has a blend profile for Ghost Protocol and V-Sync color, split, scan, warp and automation pacing.
- R3 factory effects, Canvas warp strength, Hard Light, Soft-Eye and audio-ear response adapt to the selected warp mode.
- Deterministic automation improves repeatability while preserving master levels and user-protected voice intent.
- Includes the self-contained Windows x64 application and per-user installer.

## Validation

- `dotnet publish` completed for `win-x64` with the .NET 8 SDK.
- NSIS produced the versioned installer; its file version is 6.4.17.
- The published FX files were verified against the checked-in application sources.
- All 29 warp profiles passed finite-value checks.
- The published browser app loaded with no JavaScript page errors.

## Install

Download and run `AI-WARP-ENGINE-6.4.17-x64-setup.exe` on Windows 10/11 x64.
The installer is self-contained and does not require administrator access.

**SHA-256:** `C22991E166ED288C65C11C01BFC1261C0C1625520DC0D376D2D513333CB2DD9C`
