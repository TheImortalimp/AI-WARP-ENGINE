# Windows native edition 6.4.38.1

**6.4.38.1 COMPLETE (ALL WINDOWS x64 NATIVE) -noffmpeg_(~Gyan-D)**

WinForms/WebView2 shell for the app in `../ai_warp_engine`. The media-conversion
backend uses Windows `MediaTranscoder`, not FFmpeg. This is not universal codec
support and does not imply affiliation with GyanD.

## Build

Requires .NET 8 SDK and NSIS. No FFmpeg distribution is required.

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File shell\build.ps1
Set-Location shell
& "C:\Program Files (x86)\NSIS\makensis.exe" setup.nsi
```

The project targets Windows build 19041 or newer. SDK projections restore from
the target framework. The script publishes self-contained .NET x64 into a new
`publish-native-6.4.38.1` directory, rejects an existing directory, and checks
that no FFmpeg files exist. NSIS packages that directory, never the old `publish`.

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
Unsupported codecs and corrupt inputs report explicit errors. No media is uploaded.

Converted copies are hash-named under the WebView2 profile's `media-cache`.
Native cache identity is distinct from earlier FFmpeg caches. Originals are
unchanged; cancelled/failed conversion removes partial output.

## Installation and privacy

The unsigned installer requests UAC, defaults to `Desktop\AI WARP ENGINE` and
applies existing cursor/theme/sound policies. It adds shortcuts and an uninstaller.
The app ZIP does not run installer policy scripts automatically.
Uninstall does not revert Windows policies or remove the separate user profile.
Settings/cache default to `%LOCALAPPDATA%\AI-Warp-Engine\profile`;
`AIWARP_PROFILE` may override it. Back up presets before deleting a profile.

F11 toggles fullscreen; F5 reloads. Frame capture uses HUD-free WebView2
compositor PNGs including fullscreen FX. Camera/microphone permissions are
limited to exact app documents. External-browser and new-window launches are blocked.

## App-control boundaries

Main navigation and native bridge callers are restricted to exact local app
entry documents. The LENS has no native bridge/host objects/device permissions.
Main scripts are local-only; Three.js 0.149.0 and its MIT license are bundled.
Release developer tools, browser accelerators and debugging overrides are disabled.
Accessibility remains enabled. No Defender hooks, physical-input filtering,
remote-desktop disabling or administrator-malware guarantee is included.

## Tests and older release

`node --test tests\freeze-frame.test.cjs tests\regressions.test.cjs tests\local-media.test.cjs`
and `dotnet run --project tests\AppControl` cover retained behavior.
Native conversion tests use FFmpeg only to generate/check fixtures, never as the
converter or shipped app dependency.

The older FFmpeg-based 6.4.37 Windows publication remains gated on its exact
corresponding-source materials. See [native release notes](../RELEASE_NOTES_6.4.38.1.md),
[third-party notices](../THIRD_PARTY_NOTICES.md) and
[older release gate](../WINDOWS_RELEASE_WORKFLOW.md).
