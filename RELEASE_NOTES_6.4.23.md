# AI WARP ENGINE 6.4.23 — YouTube playlist vault

Release 6.4.23 fixes playlist visibility and strengthens playlist extraction and playback handling in the standalone app.

## Highlights

- **Protected featured playlist:** the featured playlist is bundled into the YouTube handler as a protected vault entry and survives knowledgebase reset and clear-custom actions.
- **Visible playlist records:** saved playlists appear in the knowledgebase even when feed extraction fails, with direct **PLAY** and **EXTRACT** actions.
- **Stronger feed parsing:** playlist IDs are parsed from common YouTube URL forms; Atom/XML entries are read by namespace-aware element names instead of brittle prefix-sensitive regular expressions.
- **Reliable request handling:** extraction requests for the same playlist are coalesced, failures are reported, retries are time-bounded, and duplicate extraction calls were removed from the URL/paste handlers.
- **Playback fallback:** playlist-only links start direct YouTube playlist playback immediately while feed extraction runs in the background.

YouTube’s public playlist feed may return only a recent subset of a playlist. Use **PLAY** to browse the full playlist directly on YouTube. When feed access is blocked by network or CORS restrictions, the playlist remains visible and playable even though its videos cannot be imported into the local list.

## Install

Run `AI-WARP-ENGINE-6.4.23-x64-setup.exe` on Windows 10/11 x64. It installs per-user without administrator access.

**Installer SHA-256:** `E7AC44DE296A7AFC5F61C02A1F58BC3F905B7393D9F35FCD4CB9B25ACD8B475A`

[Download the AI WARP ENGINE User Manual](https://github.com/TheImortalimp/AI-WARP-ENGINE/releases/download/v6.4.23/MANUAL.md)
