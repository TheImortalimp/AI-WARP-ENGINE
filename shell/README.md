# AI WARP ENGINE — Windows app and installer

A standalone Windows shell (WinForms + Microsoft Edge WebView2) that runs `../ai_warp_engine` locally and ships as a per-user NSIS installer, the same approach as K-Meleon-R.

- Camera and microphone are allowed for exact local app documents; other native
  permission requests are denied. External-browser launches and new windows are blocked.
- F11 toggles fullscreen, F5 reloads.
- Settings and the web profile live in `%LOCALAPPDATA%\AI-Warp-Engine`.
- The 6.4.36 build repairs duplicate app documents and restores the
  dedicated YouTube LENS player without disabling cursor/theme guards. Rebuild
  the shell to copy the repaired HTML/JS assets; an old installed app does not
  pick up source-folder edits. Dependency-free regression checks:
  `node --test tests\regressions.test.cjs` from the repository root.

## Build

Requires the .NET 8 SDK, a complete FFmpeg Windows distribution
(`winget install Gyan.FFmpeg`) and NSIS (`winget install NSIS.NSIS`).

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File shell\build.ps1
Set-Location shell
& "C:\Program Files (x86)\NSIS\makensis.exe" setup.nsi
```

This produces `AI-WARP-ENGINE-6.4.37-x64-setup.exe` (gold "Я" icon, `app.ico`/`logo.png`). It installs to `Desktop\AI WARP ENGINE` with an elevated (UAC) installer that also runs `cursor_lockdown.ps1` to enforce system-only mouse pointers, and adds a launcher shortcut in that folder, a Start Menu entry and an uninstaller.

The app executable is `publish\AI-Warp-Engine.exe`; distribute the complete
publish directory, not the executable alone. .NET is self-contained, WebView2
is a runtime prerequisite. The uninstaller does not revert Windows policies
or remove the separately stored WebView2 user profile.

The **FINAL NOVEMBER 2026 RELEASE** currently distributes only the browser app
and documentation. Windows installer/app publication is pending verified FFmpeg
corresponding source; see [release notes](../RELEASE_NOTES_6.4.37.md).

The build script locates FFmpeg on PATH, or accepts `-FFmpegRoot` / the
`AIWARP_FFMPEG_ROOT` environment variable pointing to the extracted distribution.
Direct .NET builds require `-p:FFmpegRoot="C:\path\to\ffmpeg-distribution"`.
Builds fail if the executable, LICENSE or README.txt is absent; the executable
and both upstream notices are copied into `tools` and included by NSIS.
FFmpeg runs as a separate local process, only after native decoding fails. No
media is uploaded. Conversion cancellation kills only that app-owned process.

### FFmpeg redistribution

FFmpeg and its enabled libraries retain their upstream licenses; the application's
CC0/Unlicense does not cover them. Gyan's full build is GPLv3. Keep the bundled
license/build/source notices and satisfy the corresponding-source obligations
for the exact FFmpeg build and linked dependencies before distributing an
installer. See the distribution's README, <https://www.gyan.dev/ffmpeg/builds/>
and <https://ffmpeg.org/legal.html>. Installing a local test build is not a
claim that public-release licensing has been completed.

Exact local build: `8.1.2-full_build-www.gyan.dev`, FFmpeg revision
`38b88335f99e76ed89ff3c93f877fdefce736c13`, GCC 16.1.0 Rev2 (MSYS2).
The FFmpeg source commit is known, but complete linked dependency sources,
patches and build materials have not been verified. They are requested in
[GyanD/codexffmpeg #254](https://github.com/GyanD/codexffmpeg/issues/254).
Do not publish this installer or a portable bundle containing its FFmpeg binary
until the outstanding materials are obtained and reviewed.

## App control hardening

The main WebView accepts only the three local app entry documents for top-level
navigation and native bridge requests. The LENS accepts only its local player
document, has no native messaging or host objects, and denies device permissions.
Remote YouTube content remains an isolated iframe; media rendering is not a
command channel. All external-browser launches and new-window requests are
blocked, including user-clicked links. No shell protocol handler is launched.
The main app loads the pinned, bundled Three.js library rather than a CDN and
removes the unused YouTube API script. Its Content Security Policy allows only
same-origin scripts, preventing remote scripts from becoming app bridge callers.

Release builds ignore `AIWARP_DEBUG_PORT`, disable developer tools/browser
accelerators and host objects, and refuse startup when WebView2 argument or script
debugger environment overrides are set. Debug builds may use `AIWARP_DEBUG_PORT`
for local development. Registry/WebView2 runtime overrides and a compromised
Windows account are outside this application-level boundary.

Camera/microphone permissions remain available to the exact app documents;
other documents and permission kinds are denied. Generated and converted media
continue through the existing FX/audio pipeline. The converted-media host allows
CORS intentionally so the app can analyze audio and read video pixels; it contains
hash-named media copies, not app scripts or a native bridge.

No Defender hooks or scans, physical-input filtering, accessibility restrictions,
remote-desktop disabling or new system policies are included. This prevents
unauthorized web navigation/control paths; it cannot guarantee physical-only input
or defeat administrator malware or Windows remote-control software.

Validate policy rules with `dotnet run --project tests\AppControl` from the
repository root.

Bundled Three.js 0.149.0 retains its MIT license in `app\THREE-LICENSE.txt`.
Upstream: <https://github.com/mrdoob/three.js/tree/r149>.
SHA256 of `three.min.js`:
`8A5F7249903B54D30F79F708699D2FED2D6A1D0741A4CD41377D1F01BB5A2271`.
