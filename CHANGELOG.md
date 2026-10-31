# Changelog

Standalone Windows app releases. Full notes per release are in the `RELEASE_NOTES_*.md` files.

## 6.4.32 — VORTEX revamp (no black hole)
- All black hole / starfield / init-grid vortex edits removed; VORTEX restored to the 6.4.26/6.4.27 centrifugal ring base, then rebuilt: more rings across the full frame, continuous one-way flow (no back-and-forth oscillation), inner rings spin harder and are pulled inward, and log-spiral light arms in palette hues ride the flow. Audio-reactive on bass/treble.

## 6.4.31 â€” VORTEX back to the classic rings
- VORTEX restored to the original centrifugal ring vortex (with the standard chroma/tint passes), plus a new subtle black hole and a light starfield (~420 inward-drifting stars) drawn on top. Init-screen grid/oval funnel and the heavy effects were removed.

## 6.4.30 Ã¢â‚¬â€ VORTEX rebuilt on the init canvas
- VORTEX now builds on the original nested-ring vortex and the init-screen canvas: the highway grid is wrapped into a spiral funnel, the 8 revolving cyan vector ovals and magenta horizon glow orbit the black hole, with the heavy starfield on top.

## 6.4.29 ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â Real VORTEX + AI ZERO motion
- **VORTEX (FX 6)** rebuilt: differential-rotation whirlpool, event-horizon black hole with lensed accretion disk and photon ring, and a heavy starfield (~1,400 infalling streak stars + ~560 twinkling far-field stars) drawn over everything. The old twirl rings are gone.
- **AI ZERO MOTION (FX 0)** added as the first entry: no warp, no deformation, only subtle Ghost Protocol (level capped, auto-enabled as SPECTRUM if Ghost was off). Excluded from random/preset picks. Cycle label shows [0/29].
- Cursor lockdown unchanged.
## 6.4.28 ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â System-cursor-only
- The app and its LENS window always show the Windows system mouse pointer; web content cannot replace it (CSS, inline styles, shadow DOM, JS style APIs).
- Launch guard resets any pointer in `HKCU\Control Panel\Cursors` that is not empty or a `.cur`/`.ani` under `%WINDIR%\Cursors` (blocks GEM-style themed cursor sets) and re-checks on every change. Paths are normalised (8.3 names, `..`, UNC/device paths, alternate streams, symlinks, junctions).
- Process hardening: legacy extension points and remote or low-integrity image loads disabled.
- Installer now requests elevation (standard UAC prompt) and runs `cursor_lockdown.ps1` for all local profiles, with registry backups in `%ProgramData%`.
## 6.4.27 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â Hard locks, true LENS screenshots, stronger Neon/ASCII
- Freeze Frame now saves the full LENS canvas as an opaque PNG without HUD; canvas sizing no longer shrinks when zoomed out (DPR clamped, ResizeObserver).
- Voice, Speed, Opacity, Ghost and V-Sync locks are enforced every frame against Automated Masters, Continual Randomization, Timed Auto and presets.
- New Speed, Opacity and Ghost lock buttons in the bottom controls.
- Neon Gradient is a rotating multi-stop ribbon with bloom and full-frame wash; GC#(3) ASCII now streams a continual luminance-mapped character sheet.
- Twirl warp is slower, eased and smoother.

## 6.4.26 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â Full app-frame B&W and Hue FX
- Extended adjustable grayscale/inversion and Hue rotation to the entire app viewport, including the HUD and controls, in the browser edition and Windows WebView2 host.
- Kept Hard Light limited to the effects canvas and updated freeze-frame capture to preserve the viewport color treatment.

## 6.4.25 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â Full-canvas LENS inversion
- B&W now applies grayscale and inversion to the complete composed canvas, including the YT LENS feed, initialization artwork, Raum X-R showcase, and visual easter eggs.
- Hue/Color Cycle now rotates the full composed canvas instead of tinting only a translucent overlay; the browser YouTube Warp Bus uses full-strength monochrome inversion too.

## 6.4.24 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â LENS FX controls and automatic playlist extraction
- B&W strength and Hue spectrum adjustments now activate both core and GC#(3) voices, so the controls visibly affect the LENS Canvas; added monochrome and hue-spectrum previews.
- Playlist-card playback and imported playlist records now automatically start background extraction while keeping direct playlist playback available.

## 6.4.23 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â YouTube playlist vault
- Pinned the featured playlist as a protected built-in YouTube entry, preserved across reset and clear-custom actions.
- Playlist records now render in the knowledgebase with direct playback and extraction controls.
- Reworked playlist ID parsing and RSS/Atom XML extraction, added extraction request coalescing and visible failure fallback, and removed duplicate extraction/play-on-extract behavior.

## 6.4.22 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â Voice FX tuning and automation
- Added bottom-dock B&W inversion and Hue/Color Cycle fine-tune sliders with independent persistent locks and a reroll control.
- Random board rolls and preset auto-cycles randomize unlocked FX settings and independently roll Auto Glitch on/off at 50/50 odds.
- Preserved independent core and GC#(3) voices, complementary pairing, and subtle renderer differences.

## 6.4.21 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â RAUM ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â· X-R artwork show
- While a RAUM ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â· X-R track plays, the main canvas runs the cover and back-cover artwork as a cross-fading slideshow, replacing the init artwork and any other source.
- Six exclusive audio-aware glitch sequences: TEAR, GRID, MIRROR, SMEAR, COLUMNS and RIPPLE.
- Easter eggs: a giant reversed ÃƒÆ’Ã‚ÂÃƒâ€šÃ‚Â¯ during beat silence, a reversal flash on beat re-entry, a decoding caption, and a hidden SIGNATURE lattice every 64 s of track time.
- Pausing the track or starting another source restores the previous visuals.

## 6.4.20 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â RAUM ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â· X-R tracks
- Replaced the unplayable Pole ÃƒÆ’Ã¢â‚¬Å¡Ãƒâ€šÃ‚Â· R showcase with four original synthesized tracks (two jungle/breakcore, two ambient), bundled for offline, audio-reactive playback.
- Added cover art, back cover, and full credits and dedication to Stefan Betke (Pole), GitHub Copilot and River Lyle Reuveni.

## 6.4.19 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â Subtle Milkdrop blends
- Subtler DROP THE MILK? feedback, wave and bloom blending.
- Suggested YouTube playlist now plays inside the LENS with audio awareness and stops when other playback starts.

## 6.4.18 ÃƒÆ’Ã‚Â¢ÃƒÂ¢Ã¢â‚¬Å¡Ã‚Â¬ÃƒÂ¢Ã¢â€šÂ¬Ã‚Â New FX
- New FX features from the web 6.4.34 Milkdrop opacity-lock build.

## 6.4.17
- New standalone build with the added FX templates.
