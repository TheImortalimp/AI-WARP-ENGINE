# Changelog

Standalone Windows app releases. Full notes per release are in the `RELEASE_NOTES_*.md` files.

## 6.4.38.2 - NO-FFMPEG (No~Gyan-D)
- Crisp gold reversed-R vector master, ten DPI-specific ICO sizes and 1024px logo.
- New app/installer/uninstaller artwork; shortcuts retain executable icon wiring.
- Updated native edition HUD, version, cache keys and complete documentation.
- Retained .NET tribute/licensing, all voices/FX, native conversion and full-FX capture.
- Earlier releases and pending FFmpeg publication automation are unchanged.

## 6.4.38.1 - COMPLETE (ALL WINDOWS x64 NATIVE) -noffmpeg_(~Gyan-D)
- Separate FFmpeg-free Windows edition with MediaTranscoder fallback through installed OS codecs.
- Bundled .NET runtime and dependency notices; installed WebView2/media components remain prerequisites.
- Kept original files, distinct cached conversion identity, cancellation and explicit errors.
- Rejected native AVI outputs with an empty required sample track instead of reporting successful conversion.
- Retained full-FX HUD-free screenshot capture and continuous warp/media behavior.
- Explicitly carried all 34 original voices, 29 warp modes, Ghost/V-Sync and 80 Milkdrop presets from 6.4.36; checked 816 visible voice frames and 2,610 gap-free warp frames in the native host.
- Native label describes the media backend, not universal AVI support or an all-native UI.
- Validation: 11 native conversion checks, 22 control-policy assertions, converted video/audio playback with readable pixels and audio peak 198, pause/seek/resume and native/browser screenshot pixels.
- Earlier FFmpeg publication gate and private review build are unchanged.

## 6.4.37 - Glitch Hardened & FX Fixed
- Synchronized completed FX frames, continuous warp timing, 240 ms mode blending, gap coverage and last-good-frame recovery for local media and LENS.
- Bundled offline FFmpeg conversion for unsupported local video/audio, including AVI, with original preservation, cached copies, cancellation and explicit errors.
- Blocked external-browser launches, restricted app/LENS navigation and native bridge callers, disabled release debugging, and bundled Three.js with local-only script policy.
- Preserved accessibility and existing cursor protections; no Defender scanning or physical-input-only guarantee.
- Updated app, installer, metadata and HUD to `v6.4.37 · Glitch Hardened & FX Fixed`.
- Validation: 25 JavaScript regressions, 22 control-policy assertions, eight native conversion checks, and 2,610 native FX frames with no transparent/untreated frames.
- Built locally; not yet published to GitHub.

## 6.4.36 - Startup and effects regression repair
- Restored separate app, showcase and YouTube LENS documents after 6.4.35 concatenated them, duplicating controls and starting competing render loops.
- Hardened JavaScript startup with single-instance initialization, dependency/media-mount checks, bounded configuration loading and visible errors. Optional remote scripts no longer block startup.
- Render scheduling survives transient failures; voice errors restore canvas state, report visibly and leave later voices running.
- Retained Windows cursor/theme protections and aligned host/LENS validation to exact HTTPS YouTube embed origins and paths.
- Updated the title-adjacent HUD notice to `v6.4.36 · INIT + FX RESTORED · CURSOR GUARD`, with matching app/installer and asset-cache versions.
- Added dependency-free JavaScript, document-boundary, startup, render-recovery, LENS and version-consistency regression tests.

## 6.4.35 - GEM OS hardening
- **Theme/pointer package blocking:** the embedded WebView2 now cancels downloads and navigations of .theme, .themepack, .deskthemepack, .cur and .ani files (trailing dot/space tricks included) and refuses ms-settings: navigation. External links open only for http/https.
- **Runtime policy:** the guard also sets NoChangingSoundScheme alongside NoChangingMousePointers.
- **Installer:** ships and runs harden_against_gem_os.ps1 -Apply (see the GEM-OS repo). Quarantine of user files stays opt-in.
- **Limits:** an administrator can undo the policies; installers are unsigned so SmartScreen warns (code signing is the real fix); a clean scan is not proof of safety. -Apply was not run end to end on a real machine.

## 6.4.34 — Easter egg: System Protocol One OST + wider local media
- **Easter egg:** *System Protocol One OST* (PalmDesert, 14:46) is baked into the app as `audio/system-protocol-one-ost.mp4` (lossless H.264/AAC remux of the supplied .mkv, 1920x1080, cover-art stream dropped). Trigger it with the Konami code (up up down down left right left right B A) or the new egg entry in the YouTube Knowledgebase (**PLAY LOCAL**). If the local file is missing (web bundle) it falls back to the YouTube source.
- **Link verified:** https://www.youtube.com/watch?v=0YVYefCo1l4 resolves via YouTube oEmbed to *System Protocol One OST* by PalmDesert. It is a built-in KB item that survives Reset/Clear.
- **More local formats:** video .mp4 .m4v .mov .avi .mkv .webm .ogv .ogm .3gp; audio .mp3 .m4a .aac .wav .ogg .oga .opus .flac .weba .mka .spx .aif .aiff (royalty-free codecs: VP8/VP9/AV1, Opus, Vorbis, FLAC). Ogg Theora video is accepted by the picker but WebView2/Edge cannot decode it.

## 6.4.33 â€” Integration audit
- Verified in a real browser (CDP) for both the app sources and the web bundle: all 30 modes (AI ZERO + 29) switch with no JS errors, AI ZERO auto-enables a subtle SPECTRUM Ghost (0.28), random/preset picks never choose AI ZERO, select/cycle/HUD labels agree ([0/29] AI ZERO, [6/29] VORTEX), and startup stays on LIQUID.
- Fix: labels show 'AI ZERO' instead of 'AI_ZERO'.

## 6.4.32 Ã¢â‚¬â€ VORTEX revamp (no black hole)
- All black hole / starfield / init-grid vortex edits removed; VORTEX restored to the 6.4.26/6.4.27 centrifugal ring base, then rebuilt: more rings across the full frame, continuous one-way flow (no back-and-forth oscillation), inner rings spin harder and are pulled inward, and log-spiral light arms in palette hues ride the flow. Audio-reactive on bass/treble.

## 6.4.31 ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â VORTEX back to the classic rings
- VORTEX restored to the original centrifugal ring vortex (with the standard chroma/tint passes), plus a new subtle black hole and a light starfield (~420 inward-drifting stars) drawn on top. Init-screen grid/oval funnel and the heavy effects were removed.

## 6.4.30 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â VORTEX rebuilt on the init canvas
- VORTEX now builds on the original nested-ring vortex and the init-screen canvas: the highway grid is wrapped into a spiral funnel, the 8 revolving cyan vector ovals and magenta horizon glow orbit the black hole, with the heavy starfield on top.

## 6.4.29 ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€šÃ‚Â Real VORTEX + AI ZERO motion
- **VORTEX (FX 6)** rebuilt: differential-rotation whirlpool, event-horizon black hole with lensed accretion disk and photon ring, and a heavy starfield (~1,400 infalling streak stars + ~560 twinkling far-field stars) drawn over everything. The old twirl rings are gone.
- **AI ZERO MOTION (FX 0)** added as the first entry: no warp, no deformation, only subtle Ghost Protocol (level capped, auto-enabled as SPECTRUM if Ghost was off). Excluded from random/preset picks. Cycle label shows [0/29].
- Cursor lockdown unchanged.
## 6.4.28 ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€šÃ‚Â System-cursor-only
- The app and its LENS window always show the Windows system mouse pointer; web content cannot replace it (CSS, inline styles, shadow DOM, JS style APIs).
- Launch guard resets any pointer in `HKCU\Control Panel\Cursors` that is not empty or a `.cur`/`.ani` under `%WINDIR%\Cursors` (blocks GEM-style themed cursor sets) and re-checks on every change. Paths are normalised (8.3 names, `..`, UNC/device paths, alternate streams, symlinks, junctions).
- Process hardening: legacy extension points and remote or low-integrity image loads disabled.
- Installer now requests elevation (standard UAC prompt) and runs `cursor_lockdown.ps1` for all local profiles, with registry backups in `%ProgramData%`.
## 6.4.27 ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â Hard locks, true LENS screenshots, stronger Neon/ASCII
- Freeze Frame now saves the full LENS canvas as an opaque PNG without HUD; canvas sizing no longer shrinks when zoomed out (DPR clamped, ResizeObserver).
- Voice, Speed, Opacity, Ghost and V-Sync locks are enforced every frame against Automated Masters, Continual Randomization, Timed Auto and presets.
- New Speed, Opacity and Ghost lock buttons in the bottom controls.
- Neon Gradient is a rotating multi-stop ribbon with bloom and full-frame wash; GC#(3) ASCII now streams a continual luminance-mapped character sheet.
- Twirl warp is slower, eased and smoother.

## 6.4.26 ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â Full app-frame B&W and Hue FX
- Extended adjustable grayscale/inversion and Hue rotation to the entire app viewport, including the HUD and controls, in the browser edition and Windows WebView2 host.
- Kept Hard Light limited to the effects canvas and updated freeze-frame capture to preserve the viewport color treatment.

## 6.4.25 ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â Full-canvas LENS inversion
- B&W now applies grayscale and inversion to the complete composed canvas, including the YT LENS feed, initialization artwork, Raum X-R showcase, and visual easter eggs.
- Hue/Color Cycle now rotates the full composed canvas instead of tinting only a translucent overlay; the browser YouTube Warp Bus uses full-strength monochrome inversion too.

## 6.4.24 ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â LENS FX controls and automatic playlist extraction
- B&W strength and Hue spectrum adjustments now activate both core and GC#(3) voices, so the controls visibly affect the LENS Canvas; added monochrome and hue-spectrum previews.
- Playlist-card playback and imported playlist records now automatically start background extraction while keeping direct playlist playback available.

## 6.4.23 ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â YouTube playlist vault
- Pinned the featured playlist as a protected built-in YouTube entry, preserved across reset and clear-custom actions.
- Playlist records now render in the knowledgebase with direct playback and extraction controls.
- Reworked playlist ID parsing and RSS/Atom XML extraction, added extraction request coalescing and visible failure fallback, and removed duplicate extraction/play-on-extract behavior.

## 6.4.22 ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â Voice FX tuning and automation
- Added bottom-dock B&W inversion and Hue/Color Cycle fine-tune sliders with independent persistent locks and a reroll control.
- Random board rolls and preset auto-cycles randomize unlocked FX settings and independently roll Auto Glitch on/off at 50/50 odds.
- Preserved independent core and GC#(3) voices, complementary pairing, and subtle renderer differences.

## 6.4.21 ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â RAUM ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â· X-R artwork show
- While a RAUM ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â· X-R track plays, the main canvas runs the cover and back-cover artwork as a cross-fading slideshow, replacing the init artwork and any other source.
- Six exclusive audio-aware glitch sequences: TEAR, GRID, MIRROR, SMEAR, COLUMNS and RIPPLE.
- Easter eggs: a giant reversed ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚ÂÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¯ during beat silence, a reversal flash on beat re-entry, a decoding caption, and a hidden SIGNATURE lattice every 64 s of track time.
- Pausing the track or starting another source restores the previous visuals.

## 6.4.20 ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â RAUM ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â· X-R tracks
- Replaced the unplayable Pole ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã†â€™ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â· R showcase with four original synthesized tracks (two jungle/breakcore, two ambient), bundled for offline, audio-reactive playback.
- Added cover art, back cover, and full credits and dedication to Stefan Betke (Pole), GitHub Copilot and River Lyle Reuveni.

## 6.4.19 ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â Subtle Milkdrop blends
- Subtler DROP THE MILK? feedback, wave and bloom blending.
- Suggested YouTube playlist now plays inside the LENS with audio awareness and stops when other playback starts.

## 6.4.18 ÃƒÆ’Ã†â€™Ãƒâ€ Ã¢â‚¬â„¢ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬Ãƒâ€¦Ã‚Â¡ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã†â€™Ãƒâ€šÃ‚Â¢ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â€šÂ¬Ã…Â¡Ãƒâ€šÃ‚Â¬ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â New FX
- New FX features from the web 6.4.34 Milkdrop opacity-lock build.

## 6.4.17
- New standalone build with the added FX templates.
