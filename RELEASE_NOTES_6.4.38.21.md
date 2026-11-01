# 6.4.38.21 ULTIMATE

**The confluence release.** The 6.4.36 effects, the 6.4.37 continuous FX flow,
the 6.4.37.1 Freeze Frame and the native Windows media backend of 6.4.38.x,
verified together in one build, with the effects hardened and nothing left
that depends on an external media converter.

Windows x64 native-media edition with the crisp gold reversed-R icon, the .NET
tribute and licence pages, and the unchanged installer finish flow of 6.4.38.2.
The older 6.4.38.1 and 6.4.38.2 releases remain available unchanged.

## What changed

### 6.4.36 effects, proven and pinned

The 34 voices (17 core and 17 `GC#(3)`), the 29 warp modes plus AI ZERO, R3
Ghost Protocol and V-Sync, and the 80 Milkdrop presets come from the 6.4.36
sources. Four files carry them and stay byte-identical to 6.4.36 (compared
after line-ending normalisation): `glitch_engine.js`, `r3_gc3_factories.js`,
`r3_ghost_vsync.js` and `yt_lens.html`. Their SHA-256 values are pinned in the
test suite and in the engine itself, so every fix in this release lives in the
engine or the style sheet, never in a bank.

- A deeply frozen `FX_MANIFEST` lists every voice, warp mode, Ghost/V-Sync mode,
  timed cadence and bank hash.
- `verifyFxRegistry()` runs at startup and compares the live registries with the
  manifest; the result is `fxRegistryReport`.
- `attestEffectBanks()` fetches the bank files the page was served, hashes them in
  the browser and reports any mismatch. The Windows host proof runs it inside the
  real app.

### One pipeline, no external converter

Local-media conversion uses Windows `MediaTranscoder` and the codecs installed on
the PC. Nothing else is bundled, installed or launched, in the app, the build or
the tests. The native media tests generate their own fixtures and decode the
converter's output with Windows media APIs. A repository-wide test fails if a
current source, script or document names an external converter; the only
exceptions are the changelog, release notes and notices that record history, and
the older approval-gated Windows publication files, which this release leaves
untouched.

Conversion cleans up its partial file on a best-effort basis, so a locked file
can no longer replace the real error (for example a cancellation), and the
unsupported-codec message now says in plain language that no installed Windows
decoder matches the file.

### Aligned canvases

- The lens canvas, all nine effect buffers, the Tech Artist WebGL buffer and its
  resolution uniform, and the fullscreen layers share one size at every window
  size and display scaling.
- Tech Artist drew its WebGL buffer at the device pixel ratio while the engine used
  a different size, which cropped and drifted the image at 125% scaling and above.
  Its buffer now equals the lens buffer.
- A resize redraws the last picture into the new buffers, so the lens is no longer
  blank for a frame.
- If WebGL cannot start, the 2D rays fallback runs and WebGL is retried at most
  every 5 seconds instead of every frame.

### Freeze Frame without the HUD

The old mask only hid direct children of the page. Panels that were animating kept
painting through a `transition: all`, and descendants that forced their own
visibility stayed visible. The mask now covers every HUD root, all of their
descendants and the HUD children of the fullscreen layer, switches transitions and
animations off while masked, and capture waits until the HUD has settled. The
saved image still contains the effects canvas, the LENS/media layer underneath it, CRT
scanlines, vignette and the page filter once.

### HUD notifications you can see

The notification banner (blocked external links, copy and export confirmations,
playlist and LENS errors) lived inside the fullscreen effects layer, which stacks
below the control bars, so the media bar painted over it. It is now a page-level
HUD node and its existing `z-index` puts it above the bars. It still disappears
with the collapsed HUD and is still excluded from Freeze Frame.

## Verification

Every claim above has a test in the repository.

| Area | Proof |
|---|---|
| Effect banks, manifest, registry | `tests\effects-6436.test.cjs`; `voices-carryover` and `attestEffectBanks` inside the Windows app |
| Continuous FX flow | `tests\fx-continuity.browser.js` inside the Windows app |
| Canvas alignment | `tests\canvas-alignment.browser.js` across display scales, inside the Windows app |
| Freeze Frame excludes the HUD | `tests\freeze-frame.test.cjs`; magenta-sentinel `tests\freeze-frame.browser.js` inside the Windows app, which must also fail on a build with the old mask |
| HUD notifications visible | banner placement in `tests\regressions.test.cjs`; natural-position check in `tests\freeze-frame.browser.js` |
| Native media | `tests\MediaConversion` and `tests\native-media.browser.js` |
| No external converter | `tests\no-ffmpeg.test.cjs` |
| App-control boundaries | `tests\AppControl` |
| Versions and labels | `tests\regressions.test.cjs` |

`tests\NativeHost` launches the real `MainForm` with WebView2 and runs the browser
proofs. The per-build counts, logs and the red/green Freeze Frame run are in the
`AI-WARP-ENGINE-6.4.38.21-VERIFICATION.md` release asset, next to `SHA256SUMS.txt`.
Uploaded asset sizes and SHA-256 values are verified before the release is made
public.

## Runtime and codec requirements

Requires Windows 10 build 19041+/Windows 11 x64, installed WebView2 and Windows
media components. .NET 8 x64 is bundled. Windows N needs the Media Feature Pack.
No codec pack or WebView2 installer is bundled.

Local media plays directly when WebView2 supports it. Otherwise Windows converts
it to H.264/AAC MP4 or AAC M4A with the codecs installed on the PC. Files Windows
cannot decode, such as AVI files with codecs the PC lacks, fail with an explicit
error and the original is never changed. The label describes the media backend; it
does not promise every codec or an all-native UI.

## Website bundle

`AI-WARP-ENGINE-6.4.38.21-web-upload.zip` holds the updated root `home.html` and
`index.html` and the complete `ai_warp_engine/` folder with the gold favicon.
Other site sections are preserved. Back up the site first and extract into the
imortalimp.nl document root. It is an update bundle, not a site backup, and
publishing on GitHub does not deploy imortalimp.nl.

## Installer and protections

The unsigned installer requests UAC and retains the .NET team/community tribute,
the .NET runtime MIT-licence page and the separate Windows SDK terms. Cursor,
theme and sound policy scripts are unchanged; the complete app ZIP does not run
them. Hardening is at the app layer: no OS-level anti-hijacking, Defender
scanning or physical-input filtering is promised, and an administrator can undo
the policies.

## Build

1. `python shell\build-icon.py` (only to regenerate artwork)
2. `powershell.exe -NoProfile -ExecutionPolicy Bypass -File shell\build.ps1`
3. Compile `shell\setup.nsi` with NSIS.

The release includes the x64 installer, the complete self-contained app ZIP, the
browser app folder ZIP, the exact committed source ZIP with its manifest, the
documentation ZIP, the website update ZIP, the icon pack, the verification report
and SHA256 checksums. The manifests identify the exact build commit; no
independently reproducible compiler-output claim is made.

## Earlier releases

The 6.4.37 and 6.4.37.1 Windows binaries remain withheld pending source review of
a bundled third-party component, and the approval-gated publication workflow and
private review build are unchanged. See [third-party notices](THIRD_PARTY_NOTICES.md).
