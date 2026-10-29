# Changelog

Standalone Windows app releases. Full notes per release are in the `RELEASE_NOTES_*.md` files.

## 6.4.26 — Full app-frame B&W and Hue FX
- Extended adjustable grayscale/inversion and Hue rotation to the entire app viewport, including the HUD and controls, in the browser edition and Windows WebView2 host.
- Kept Hard Light limited to the effects canvas and updated freeze-frame capture to preserve the viewport color treatment.

## 6.4.25 — Full-canvas LENS inversion
- B&W now applies grayscale and inversion to the complete composed canvas, including the YT LENS feed, initialization artwork, Raum X-R showcase, and visual easter eggs.
- Hue/Color Cycle now rotates the full composed canvas instead of tinting only a translucent overlay; the browser YouTube Warp Bus uses full-strength monochrome inversion too.

## 6.4.24 — LENS FX controls and automatic playlist extraction
- B&W strength and Hue spectrum adjustments now activate both core and GC#(3) voices, so the controls visibly affect the LENS Canvas; added monochrome and hue-spectrum previews.
- Playlist-card playback and imported playlist records now automatically start background extraction while keeping direct playlist playback available.

## 6.4.23 — YouTube playlist vault
- Pinned the featured playlist as a protected built-in YouTube entry, preserved across reset and clear-custom actions.
- Playlist records now render in the knowledgebase with direct playback and extraction controls.
- Reworked playlist ID parsing and RSS/Atom XML extraction, added extraction request coalescing and visible failure fallback, and removed duplicate extraction/play-on-extract behavior.

## 6.4.22 — Voice FX tuning and automation
- Added bottom-dock B&W inversion and Hue/Color Cycle fine-tune sliders with independent persistent locks and a reroll control.
- Random board rolls and preset auto-cycles randomize unlocked FX settings and independently roll Auto Glitch on/off at 50/50 odds.
- Preserved independent core and GC#(3) voices, complementary pairing, and subtle renderer differences.

## 6.4.21 — RAUM · X-R artwork show
- While a RAUM · X-R track plays, the main canvas runs the cover and back-cover artwork as a cross-fading slideshow, replacing the init artwork and any other source.
- Six exclusive audio-aware glitch sequences: TEAR, GRID, MIRROR, SMEAR, COLUMNS and RIPPLE.
- Easter eggs: a giant reversed Я during beat silence, a reversal flash on beat re-entry, a decoding caption, and a hidden SIGNATURE lattice every 64 s of track time.
- Pausing the track or starting another source restores the previous visuals.

## 6.4.20 — RAUM · X-R tracks
- Replaced the unplayable Pole · R showcase with four original synthesized tracks (two jungle/breakcore, two ambient), bundled for offline, audio-reactive playback.
- Added cover art, back cover, and full credits and dedication to Stefan Betke (Pole), GitHub Copilot and River Lyle Reuveni.

## 6.4.19 — Subtle Milkdrop blends
- Subtler DROP THE MILK? feedback, wave and bloom blending.
- Suggested YouTube playlist now plays inside the LENS with audio awareness and stops when other playback starts.

## 6.4.18 — New FX
- New FX features from the web 6.4.34 Milkdrop opacity-lock build.

## 6.4.17
- New standalone build with the added FX templates.
