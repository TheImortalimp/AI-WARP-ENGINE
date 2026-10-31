# AI WARP ENGINE 6.4.28 — System-cursor-only

## Highlights

- **System mouse only:** web content and LENS pages can no longer replace the pointer; the Windows system pointers are always shown.
- **Launch guard:** non-system pointer entries (anything not empty or under `%WINDIR%\Cursors`, e.g. a GEM-style theme) are reset on launch and on every change.
- **Process hardening:** legacy extension points (AppInit/global hook DLLs, legacy IMEs) and remote or low-integrity image loads disabled.
- **Installer:** requests elevation (standard UAC prompt) and runs `cursor_lockdown.ps1` to lock pointer changes for all local profiles; backups go to `%ProgramData%`.

## Install

Run `AI-WARP-ENGINE-6.4.28-x64-setup.exe` on Windows 10/11 x64. The installer is unsigned; SmartScreen may warn (More info, Run anyway).
