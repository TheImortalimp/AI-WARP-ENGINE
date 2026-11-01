# Windows native edition 6.4.38.21

**6.4.38.21 ULTIMATE**

WinForms/WebView2 shell for the app in `../ai_warp_engine`. The media-conversion
backend is Windows `MediaTranscoder`. This is not universal codec support.

## Build

Requires .NET 8 SDK and NSIS. No external media tool or codec distribution is required.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File shell\build.ps1
Set-Location shell
& "C:\Program Files (x86)\NSIS\makensis.exe" setup.nsi
```

The project targets Windows build 19041 or newer. SDK projections restore from
the target framework. The script publishes self-contained .NET x64 into a new
`publish-native-6.4.38.21` directory, rejects an existing directory, and refuses
to continue if the output contains any media-converter executable. NSIS packages
that directory, never an older publish directory.

For a clean rebuild, pass `-OutputDirectory` to the build script and the same
directory via NSIS `/DSRC=...`. Existing outputs are never overwritten by the
publish script. The app, installer, uninstaller and shortcuts retain the
gold reversed-R `app.ico`, regenerated from `app-icon.svg` with ten sizes:
16, 20, 24, 32, 40, 48, 64, 96, 128 and 256 pixels. The 1024px `logo.png`
is generated from the same vector master. Rebuild with
`python shell\build-icon.py` (requires Pillow); no artwork service is used.
Supersampled curves and DPI-specific sizes avoid enlarging a tiny icon.
Installer licensing includes a .NET team/community tribute and a separate
page showing the bundled .NET runtime's actual MIT license, followed by Windows
SDK terms. Runtime notices remain in the installed app.

Run `AI-Warp-Engine.exe` with all adjacent DLLs/runtime files and the `app` assets.
WebView2 and Windows media components must already be installed. Windows N
requires the Media Feature Pack; licensed codec extensions are separate.
No Windows codec pack or WebView2 runtime installer is bundled.

## Native conversion

WebView2 playback is attempted first. Unsupported local-media decoding requests
attach one user-selected `File` object to the native bridge. Windows
MediaTranscoder converts supported files to H.264/AAC MP4 or AAC M4A.
It retains source dimensions rounded up to even values, using the HD720p
profile's baseline bitrate/frame-rate settings. Audio uses stereo 48 kHz AAC.
Unsupported codecs and corrupt inputs report explicit errors that name the
Windows component that could not decode them. No media is uploaded.

Converted copies are hash-named under the WebView2 profile's `media-cache`
using the `windows-native-h264-aac-v2` identity. Originals are unchanged;
cancelled/failed conversion removes partial output on a best-effort basis
without hiding the original error.

## Installation and privacy

The unsigned installer requests UAC, defaults to `Desktop\AI WARP ENGINE` and
applies existing cursor/theme/sound policies. It adds shortcuts and an uninstaller.
The app ZIP does not run installer policy scripts automatically.
Uninstall does not revert Windows policies or remove the separate user profile.
Settings/cache default to `%LOCALAPPDATA%\AI-Warp-Engine\profile`;
`AIWARP_PROFILE` may override it. Back up presets before deleting a profile.

F11 toggles fullscreen; F5 reloads. Frame capture uses WebView2 compositor PNGs
that include fullscreen FX and exclude the whole HUD: the HUD is masked,
including descendants and in-flight transitions, then restored. Camera/microphone
permissions are limited to exact app documents. External-browser and new-window
launches are blocked.

## App-control boundaries

Main navigation and native bridge callers are restricted to exact local app
entry documents. The LENS has no native bridge/host objects/device permissions.
Main scripts are local-only; Three.js 0.149.0 and its MIT license are bundled.
Release developer tools, browser accelerators and debugging overrides are disabled.
Accessibility remains enabled. No Defender hooks, physical-input filtering,
remote-desktop disabling or administrator-malware guarantee is included.

## Tests

- `node --test tests\*.test.cjs` covers the effect manifest and pinned bank
  hashes, freeze-frame masking, version and label consistency, local-media
  routing, and a guard that keeps external converters out of the tree.
- `dotnet run --project tests\AppControl` covers the app-control boundaries.
- `dotnet run --project tests\MediaConversion -- <empty-directory>` generates
  its own fixtures and decodes the converter's output with Windows media APIs.
- `tests\NativeHost` launches the real `MainForm` with WebView2 and runs the
  in-page proofs (FX continuity, voice carry-over, canvas alignment,
  freeze-frame HUD exclusion and native media). See its README.

The 6.4.37 and 6.4.37.1 Windows binaries remain gated on a source review of
their bundled third-party component. See the [release notes](../RELEASE_NOTES_6.4.38.21.md),
[third-party notices](../THIRD_PARTY_NOTICES.md) and
[older release gate](../WINDOWS_RELEASE_WORKFLOW.md).
