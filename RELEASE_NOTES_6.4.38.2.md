# 6.4.38.2 NO-FFMPEG (No~Gyan-D)

**FINAL NOVEMBER FIXED RELEASE**

## Final website and documentation publication

Includes `AI-WARP-ENGINE-6.4.38.2-web-upload.zip`: updated root `home.html`/
`index.html` and complete `ai_warp_engine/` with the gold favicon and HUD fix.
Back up the site and extract into the imortalimp.nl document root. Other site
sections are preserved. This is an update bundle, not a complete site backup,
and GitHub publication does not automatically deploy imortalimp.nl.

Full About, manual, changelog, third-party notices, release notes and runtime
license documents accompany the release. A separate website-source ZIP/manifest
identifies the final website/documentation commit. Existing Windows source/app
manifests and verified executables retain their build revision.

35 web/FX/media regression checks passed. The user confirmed the elevated
installer and launched app were no longer clipped; all 288 installed app files
matched the published manifest, with no FFmpeg files. All release asset sizes
and SHA256 digests are checked after upload.

Windows x64 native-media edition with a crisp gold reversed-R icon.
The older 6.4.38.1 release remains available unchanged.

## Crisp icon

- Redrawn scalable master: [shell/app-icon.svg](shell/app-icon.svg).
- The same gold reversed-R identity, dark background and rounded gold border
  are preserved, with clean curves and antialiased edges.
- Ten dedicated Windows icon sizes: 16, 20, 24, 32, 40, 48, 64, 96, 128, 256px.
  These cover taskbar, shortcuts, Explorer and common high-DPI scales.
- A 1024px PNG logo is generated from the same master.
- App, installer and uninstaller embed the new icon; shortcuts use the app icon.
- Rebuild artwork with `python shell\build-icon.py` (Pillow required).
  No third-party image generation service is involved.

Windows may retain an old shortcut icon in its cache. Newly installed shortcuts
use the new executable; unpin/re-pin an old taskbar shortcut if it stays stale.
The installer does not forcibly clear system-wide icon caches.

## Retained features

All 34 core/GC3 voices, 29 warp modes, Ghost/V-Sync and the 80-preset Milkdrop
deck remain intact. Continuous-frame recovery, crossfades, clean cached media,
HUD-free full-scope freeze-frame screenshots and app-control protections are
unchanged. The icon update does not alter rendering or media conversion.

The preceding native host validation exercised 816 visible voice frames and
2,610 warp frames without transparent/untreated gaps. The original voice
and R3 banks remain unchanged from 6.4.36.

## Runtime and codec requirements

Requires Windows 10 build 19041+/Windows 11 x64, installed WebView2 and Windows
media components. .NET 8 x64 is bundled. Windows N needs the Media Feature Pack.
No FFmpeg binary/library, Windows codec pack or WebView2 installer is bundled.
The label does not imply GyanD affiliation or universal AVI support.

Local media plays directly when WebView2 supports it. Otherwise Windows
MediaTranscoder attempts H.264/AAC MP4 or AAC M4A using installed OS codecs.
Unsupported/corrupt inputs fail explicitly; AVI output without video samples is
rejected. Originals remain unchanged and cancelled conversions remove partials.
The native media-cache identity and conversion profile are unchanged.

## Installer licensing

### Launch-flow clipping fix

The refreshed 6.4.38.2 installer uses short explicit welcome/finish headings,
completion text and "Launch AI WARP ENGINE" checkbox text instead of inserting
the long edition name into fixed-size wizard controls. The app's startup window
is centered and constrained to its monitor's working area after DPI scaling,
so its borders do not extend below the taskbar on smaller/high-DPI displays.
The HUD title/version group wraps within the viewport instead of overflowing.
The refreshed app/source manifests identify the new build revision; the original
release tag is retained. Use the refreshed checksum file for these downloads.

The installer retains the .NET team/open-source community tribute, the actual
.NET runtime MIT-license page, and separate Windows SDK terms.
Exact runtime notices, WebView2 SDK notices and NAudio/Three.js licenses remain
bundled. No Microsoft endorsement is implied. App CC0/Unlicense terms do not
replace third-party component or media rights.

The unsigned installer requests UAC and retains existing cursor/theme policy
scripts. The complete app ZIP does not run installer policy scripts.
Neither edition promises OS-level anti-hijacking protection.

## Build and release verification

1. `python shell\build-icon.py`
2. `powershell.exe -NoProfile -ExecutionPolicy Bypass -File shell\build.ps1`
3. Compile `shell\setup.nsi` with NSIS.

The release includes the x64 installer, complete self-contained Windows app ZIP,
browser app folder ZIP, exact committed source ZIP, source/app manifests, full
documentation and SHA256 checksums. Root website home/index pages are excluded;
the app's own entry HTML is retained. Manifests identify the exact build commit;
no independently reproducible compiler-output claim is made.

Validation checks all ten PNG icon entries and dimensions, verifies every icon
image inside the compiled app/installer against the ICO, checks bundled notices,
version/HUD/cache consistency, and rejects FFmpeg payloads. Rendering/media/control
regressions run alongside these checks. Uploaded asset sizes and SHA256 digests
are verified before public release.

The older FFmpeg-based 6.4.37/6.4.37.1 publication gate and private Gyan review
assets/workflow remain unchanged.
