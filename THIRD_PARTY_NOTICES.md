# Third-party notices and redistribution status

AI WARP ENGINE app code uses CC0 / The Unlicense. Those terms do not replace
third-party software licenses or media rights.

## Browser distribution

- Three.js 0.149.0 (r149): MIT; retain
  [THREE-LICENSE.txt](ai_warp_engine/THREE-LICENSE.txt).
  Upstream: <https://github.com/mrdoob/three.js/tree/r149>.
  `three.min.js` SHA256:
  `8a5f7249903b54d30f79f708699d2fed2d6a1d0741a4cd41377d1f01bb5a2271`.
- Bundled RAUM audio/artwork and other media retain their respective rights and
  credits; see [About](ABOUT.md#credits-and-license) and the browser README.
  No third-party recordings or affiliation are asserted.

## Windows distribution pending source review

The local Windows installer includes Gyan's full static GPLv3 FFmpeg build and
its LICENSE/README under `tools`. GPLv3 applies to that binary and its covered
linked dependencies, not by virtue of app CC0 licensing.

Verified provenance:

| Item | Value |
|---|---|
| FFmpeg version | `8.1.2-full_build-www.gyan.dev` |
| FFmpeg revision | `38b88335f99e76ed89ff3c93f877fdefce736c13` |
| Compiler | GCC 16.1.0 Rev2 (MSYS2) |
| Configuration | Static, `--enable-gpl --enable-version3`; full configuration in upstream README |
| Binary SHA256 | `ad8f211bc894755e0061c55ab280ae00e8d3d4f15a8cc4372b24cfa247b5942e` |
| Source request | <https://github.com/GyanD/codexffmpeg/issues/254> |

Upstream:
<https://github.com/GyanD/codexffmpeg/releases/tag/8.1.2> and
<https://github.com/FFmpeg/FFmpeg/commit/38b88335f99e76ed89ff3c93f877fdefce736c13>.

**Not yet verified:** exact sources for all static/transitive dependencies,
matching patches, build/install recipes and toolchain/package inputs.
The upstream FFmpeg commit and build configuration alone are not a verified
complete corresponding-source package. No claim of completed redistribution
compliance is made. Windows installer and portable app assets remain unpublished.

Before Windows publication, obtain and review those materials, retain their
notices, provide the required corresponding source with the distributed binary,
and verify source coverage matches the binary actually shipped.

The Windows app also uses Microsoft .NET and WebView2 components under their
upstream terms. Preserve runtime notices in a complete app package. .NET is
self-contained; WebView2 remains a prerequisite, not a bundled browser installer.
