# AI WARP ENGINE — Windows app and installer

A standalone Windows shell (WinForms + Microsoft Edge WebView2) that runs `../ai_warp_engine` locally and ships as a per-user NSIS installer, the same approach as K-Meleon-R.

- Camera, microphone and screen capture are auto-allowed for the app's own origin only; external links open in the default browser.
- F11 toggles fullscreen, F5 reloads.
- Settings and the web profile live in `%LOCALAPPDATA%\AI-Warp-Engine`.

## Build

Requires the .NET 8 SDK and NSIS (`winget install NSIS.NSIS`).

```powershell
cd shell
dotnet publish -c Release -r win-x64 --self-contained true -o publish
& "C:\Program Files (x86)\NSIS\makensis.exe" setup.nsi
```

This produces `AI-WARP-ENGINE-6.4.27-x64-setup.exe` (gold "Я" icon, `app.ico`/`logo.png`). It installs to `Desktop\AI WARP ENGINE` with no admin rights, and adds a launcher shortcut in that folder, a Start Menu entry and an uninstaller.
