# AI WARP ENGINE 6.4.36 - Init + FX restored, cursor guard retained

## Regression repair

6.4.35 accidentally concatenated the app, showcase and YouTube LENS HTML
documents. Repeated IDs, canvases and script stacks created competing engines,
hid the initialization artwork and disrupted effects/control binding.

- Restored each app alias to one document and one ordered engine stack.
- Restored the animated initialization grid/rings and responsive effect playback.
- Restored `ai_warp_showcase_section.html` as an independent showcase fragment.
- Restored `yt_lens.html` as the dedicated YouTube player.
- Aligned native host and LENS URL validation to exact HTTPS YouTube embed
  origins/paths, without credentials or nonstandard ports.

## JavaScript hardening

- Single-instance startup prevents duplicate integration scripts from starting
  competing engines.
- Required engine dependencies and unique media mounts are checked explicitly.
- Configuration requests time out after five seconds; failure emits an explicit
  warning before built-in defaults are used.
- Optional remote libraries no longer block the local engine script stack.
- Startup, JavaScript runtime, asynchronous and voice-render errors are visible.
- Animation is queued before rendering so transient failures do not cancel
  later frames. Failed voice passes restore their Canvas state and do not
  prevent later voices from running.
- Automation delta is bounded when resuming from background-tab suspension.

## Cursor confluence and version notice

Windows process/system-pointer protection and theme/pointer guards remain enabled
in both app and LENS views. The guard was tested alongside effect/control style
updates; unsafe custom cursor imagery is overridden without blocking opacity,
transform or color filters.

The title-adjacent HUD notice reads
**v6.4.36 - INIT + FX RESTORED - CURSOR GUARD**. Host, installer, app metadata,
showcase links and cache-busted assets are synchronized to this release.

## Downloads

- `AI-WARP-ENGINE-6.4.36-x64-setup.exe` - self-contained Windows x64 installer,
  including bundled media.
- `AI-WARP-ENGINE-6.4.36-browser.zip` - matching `ai_warp_engine/` folder,
  including media; extract and serve over localhost.
- `SHA256SUMS.txt` - SHA-256 checksums for the installer and browser ZIP.
- `RELEASE_NOTES_6.4.36.md` - these notes.

The installer requires UAC elevation and applies Windows cursor/theme/sound
policy hardening. It is unsigned and may trigger SmartScreen. Browser use does
not apply Windows policies.

## Validation

- 11 dependency-free regression tests passed.
- All application/web JavaScript and inline scripts parsed.
- Browser checks rendered all 34 voices and exercised all 30 mode selections
  without JavaScript errors.
- Initialization artwork was visibly restored; randomize controlled the active
  engine while animation continued.
- Duplicate integration-script execution retained the same engine instance.
- A stalled configuration request fell back after approximately five seconds,
  preserving 34 voices and 80 machine presets.
- Injected native cursor guard preserved startup, normal pointer semantics and
  effect-related opacity/transform/filter updates.
- Windows Release build, self-contained publish and NSIS packaging succeeded.
  Three existing nullable-annotation warnings remain.
- Published HTML/JS/CSS/JSON were compared byte-for-byte with source.

Native LENS/audio capture and machine-policy installation are not certified
end-to-end by these browser/build checks. YouTube embedding and codec restrictions
remain external limitations. Administrative users can change Windows policies.
