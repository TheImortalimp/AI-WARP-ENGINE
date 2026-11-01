# NativeHost - real-host browser test harness

Runs the **real** AI WARP ENGINE desktop host from a *published folder* and drives it:
the WinForms `AiWarpEngine.MainForm` with its WebView2 control is created in-process, the page
`https://warp.local/index.html` loads exactly as in the shipped exe, your browser-side test files
(`tests\*.browser.js`) are injected into that page, one entry-point expression is awaited, and the
verdict is written as JSON. Nothing is emulated: the WebView2 runtime, virtual host mappings,
native bridge (`window.chrome.webview`) and window DPI are the app's own.

`Program.Main` of the app is **not** called. The harness loads `<publishDir>\AI-Warp-Engine.dll`,
points `AppContext.BaseDirectory` at the publish folder and constructs `MainForm` by reflection, so
the publish folder is used in place (it is never copied).

## Usage

Run from the repository root (Windows, interactive desktop session):

```powershell
$app = "shell\publish-native-6.4.38.21"   # any published build folder
$fresh = { "tests\NativeHost\bin\nh-profile-" + [guid]::NewGuid().ToString("N") }   # a new empty profile per run

dotnet run --project tests\NativeHost -c Release -- --app $app --profile (& $fresh) --out nh-result.json `
  --script tests\fx-continuity.browser.js --call "runFxContinuityChecks()" --size 1338x804

dotnet run --project tests\NativeHost -c Release -- --app $app --profile (& $fresh) --out nh-result.json `
  --script tests\freeze-frame.browser.js --call "runFreezeFrameChecks()" --size 1338x804
```

| Option | Meaning |
| --- | --- |
| `--app <dir>` | Published folder that contains `AI-Warp-Engine.dll` (required). |
| `--profile <dir>` | WebView2 profile, passed to the app as `AIWARP_PROFILE`; created if missing. Use a fresh folder per run (required). |
| `--out <file>` | Result JSON (required). |
| `--script <file>` | Classic script injected before the call; repeatable, injected in order. |
| `--call "<expr>"` | JS expression awaited in the page, e.g. `runVoiceCarryoverChecks()` (required). |
| `--size WxH` | Window client size in physical pixels. Default: the app's own size. |
| `--timeout <s>` | Seconds to wait for the call (default 180). |
| `--media <dir>` | Files copied into `<profile>\media-cache` before startup (served as `https://media.warp.local/`). |
| `--shot <png>` | Window screenshot via `CapturePreviewAsync` before closing (debug aid). |
| `--verbose` | Progress log on stderr. |

## Proofs

| Script | `--call` | Proves |
| --- | --- | --- |
| `tests\fx-continuity.browser.js` | `runFxContinuityChecks()` | 29 warp modes at three strengths without transparent, untreated or reset frames; failure recovery. |
| `tests\voices-carryover.browser.js` | `runVoiceCarryoverChecks()` | All 34 voices, Ghost/V-Sync systems, the 80 Milkdrop presets, the FX registry and the four effect-bank hashes. |
| `tests\canvas-alignment.browser.js` | `runCanvasAlignmentChecks()` | Lens, nine effect buffers, Tech Artist and fullscreen layers aligned across display scales and resizes. |
| `tests\freeze-frame.browser.js` | `runFreezeFrameChecks()` | Magenta-sentinel proof that no HUD pixel reaches the freeze frame; it fails on a build with the old mask. |
| `tests\native-media.browser.js` | `runNativeMediaChecks('<videoUrl>','<audioUrl>')` | Converted video and audio decode, reach the FX canvas and play under transport controls. |

The media proof needs files converted by the Windows backend. Generate them with the
conversion test, which prints `videoName`, `audioName` and `cache` in its JSON summary, and serve them
through `--media`:

```powershell
dotnet run --project tests\MediaConversion -- "<empty fixture directory>"
dotnet run --project tests\NativeHost -c Release -- --app $app --profile (& $fresh) --out nh-result.json `
  --media "<fixture directory>\cache" --script tests\native-media.browser.js `
  --call "runNativeMediaChecks('https://media.warp.local/<videoName>','https://media.warp.local/<audioName>')"
```

Flow: wait for WebView2 + first navigation, poll (250 ms, up to 30 s) for `window.aiWarpEngine`
(otherwise `engine not ready`), inject the scripts, run
`(async()=>{try{window.checkResult={ok:true,value:await (<call>)}}catch(e){window.checkResult={ok:false,error:String(e&&e.stack||e)}}})()`
and poll `window.checkResult` until it is set or `--timeout` expires (`timeout`).

## Result

`{ "ok", "value", "error", "elapsedMs", "webview2Version", "clientSize": [w,h], "devicePixelRatio" }`, plus
`viewport`, `startupMs`, `callMs`, `shot`, `warnings`, `diagnostics` and `hostTrace` when available.
`clientSize` is the real window client area in device pixels, `devicePixelRatio` is read before the
tests run (tests may override it afterwards). Exit code `0` when `ok`, otherwise `1`. The JSON is
written before the window is closed, and every error (usage, startup, test) is reported as `ok:false`.

## Notes

- Never set `WEBVIEW2_*` environment variables: Release builds of the app refuse to start with them,
  and the harness stops immediately when one of those is set.
- The harness and the app must use the same `Microsoft.Web.WebView2` package version (1.0.2792.45,
  see `NativeHost.csproj`); otherwise the app's `view` field is a different type and the harness says so.
  Other dependencies (NAudio, ...) are resolved from the publish folder by `Assembly.LoadFrom` and a
  `Resolving` handler.
- The window must stay visible and not minimized. It is created on the monitor with the lowest scale factor
  where the requested client size fits (so CSS pixels equal device pixels when possible); a framed window can
  never be larger than Windows allows on a monitor, in which case `clientSize` is smaller than requested and
  a warning is recorded. With several monitors, a request that does not fit the lowest-scale one opens on the
  larger one (a 1920x1080 request becomes a 1920x1067 client on a 1080p display).
- A blocking startup dialog of the app is closed and reported as the error. On exit any `msedgewebview2`
  child left behind is terminated, so no process outlives the run.
