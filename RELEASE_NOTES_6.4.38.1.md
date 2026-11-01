# 6.4.38.1 COMPLETE (ALL WINDOWS x64 NATIVE) -noffmpeg_(~Gyan-D)

Separate FFmpeg-free Windows x64 edition. This release does not unblock or
replace the pending FFmpeg-based 6.4.37 Windows release or private 6.4.37.1.

## What is native, and what is bundled

- The app's unsupported-local-media fallback uses Windows `MediaTranscoder`
  (Windows Media Foundation/codecs), not an FFmpeg executable or library.
- .NET 8 x64 runtime and app dependencies/assets are bundled.
- Microsoft Edge WebView2 must already be installed.
- Windows 10 version 2004/build 19041 or newer, or Windows 11 x64, is required.
- Windows media components and codecs come from the installed OS.
  Windows N editions require Microsoft's Media Feature Pack.
  Licensed codec extensions, if needed, are installed separately from Microsoft.
  No Windows codec pack or WebView2 installer is included.

**"COMPLETE (ALL WINDOWS x64 NATIVE)" describes this edition's native media
backend and complete app bundle, not support for every media file.** The UI is
still a WebView2 JavaScript/Canvas app. No affiliation with GyanD is implied.

## Local media behavior

Native WebView2 playback is tried first. Decode/container failures trigger
Windows conversion to H.264/AAC MP4 or AAC M4A, stereo 48 kHz AAC at 192 kbps.
Video dimensions are rounded up to even values; the native HD720p profile sets
the baseline encoder bitrate/frame-rate defaults, with source dimensions retained.
It is a playback-compatible transcode, not archival/lossless conversion.

Conversion only works when installed Windows decoders and encoders support the
input and output. AVI is a container: some AVI codecs work, others do not.
Unsupported codecs, corrupt files and missing OS media support report explicit
errors; the app does not pretend conversion succeeded.
An installed Windows decoder can sometimes yield an audio-only MP4 while
reporting a completed AVI conversion. The app checks the required output track's
sample table and rejects empty video/audio rather than caching a broken file.

Original files remain untouched. Converted copies use a versioned SHA256 cache
under the WebView2 profile. Source changes cancel pending conversion and partial
output is deleted. FFmpeg caches use a different identity and are not reused.
Conversion is offline; no media is uploaded.

## Retained features

Continuous warp timing, synchronized completed FX frames, mode crossfades,
last-good-frame recovery and clean local-video buffering remain.
Freeze Frame captures full composited LENS/local-media FX plus CRT/vignette
and page filters without HUD. Browser YouTube still requires tab/screen capture.
The native control/navigation/permission boundaries and blocked browser launches
remain. Accessibility remains enabled; no Defender scans or physical-only input
guarantee is added.

## Installation and app bundle

Use `AI-WARP-ENGINE-6.4.38.1-x64-setup.exe` or extract the complete Windows app ZIP.
Do not run the EXE separated from its DLLs, `app` and runtime files.
The installer is unsigned, requests UAC and applies existing Windows
cursor/theme/sound policies. The portable app ZIP does not run installer policy
scripts automatically, but retains the app's runtime cursor guard.
Uninstall does not automatically revert policies or delete the separate profile.
See MANUAL and ABOUT before installation.

The package intentionally contains no `ffmpeg`, `ffprobe`, `ffplay`, codec pack
or WebView2 runtime installer. Public distribution of this edition does not
depend on the pending Gyan FFmpeg source request. Other included dependencies
retain their upstream licenses/notices.

## Build

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File shell\build.ps1
Set-Location shell
& "C:\Program Files (x86)\NSIS\makensis.exe" setup.nsi
```

Requires .NET 8 SDK (Windows SDK projection restores from the project target)
and NSIS. The build uses a new `publish-native-6.4.38.1` directory to avoid
accidentally shipping stale FFmpeg from an earlier publish directory.
The script rejects an already-existing output directory and FFmpeg files.

Checksums, matching source and full documentation accompany the release.
The old pending-release workflow remains pinned to the original 6.4.37 installer;
it must never copy this edition into that release without an intentional change.

## Validation

### Installer branding and .NET tribute refresh

The refreshed downloads retain the existing gold reversed-R app icon in the
Windows app, installer and uninstaller, with shortcuts using the app's icon.
The installer licensing screen thanks the Microsoft .NET team and open-source
contributors without implying endorsement. A separate page presents the actual
bundled .NET runtime MIT license; exact runtime licenses/notices remain packaged.
This is a same-version branding/licensing refresh. The release tag identifies
the original publication; refreshed source/app manifests identify the exact
new build revision. Use the refreshed checksums with the refreshed downloads.

11 native conversion checks passed for MP4, WAV, silent video, cache reuse,
original preservation, cancellation, corrupt/unsupported inputs and AVI
empty-video rejection. An MJPEG AVI on this machine could not produce a valid
video track and was rejected explicitly; do not infer general AVI support.
22 app-control assertions passed.
The real Windows host played native-converted 160 x 90 video with readable
Canvas pixels, nonzero converted-audio analysis (peak 198), pause/seek/resume
and full compositor screenshots. Native/browser screenshot center and edge
colors agreed within one channel value and inversion was applied once.

## Explicit 6.4.36 FX/voice carryover

The original glitch engine, GC3 factory bank and R3 Ghost/V-Sync bank are
unchanged from the 6.4.36 checkout (`967be9cb2b35d1a9f4a050c2cede32ca86565e7b`).
The later continuous-frame, transition and full-scope screenshot fixes remain
around those banks; switching the media backend does not replace them.

All 17 core voices are carried in: Neon Grad, Scanlines, Data Bands, Flash,
HSL Rainbow, ASCII, Font Burn, Strobe, Gemini Bomb, Tech Artist, VHS Glitch,
Data Bomb, Ink Glyphs, Matrix ASCII, Matrix Chord, B&W Invert and Hue/Color Cycle.
Their 17 independently controlled `GC#(3)` counterparts are also carried in,
for 34 voices total, with level, size, depth and user-disable controls.

The native host exercised every voice for 24 frames (816 voice frames total),
requiring observable pixels or an active display filter and no render errors.
Ghost DIA/SPECTRUM/CRT/PLASMA/LED and V-Sync ON/GHOST_OFF/FULL were selected
and rendered successfully, with the complete 80-preset Milkdrop deck retained.
The continuity test sampled 2,610 frames across 29 warp modes at three strengths:
zero transparent frames, zero untreated frames, no steady-state scratch-buffer
resizes, immediate OFF/AI ZERO and recovery from an injected rendering failure.
These are measured test results, not a guarantee for every GPU or codec.
