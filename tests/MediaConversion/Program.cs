using AiWarpEngine;
using System.Diagnostics;
using System.Text.Json;

if (args.Length != 2) throw new ArgumentException("Supply the FFmpeg executable and an empty fixture directory.");
var ffmpeg = Path.GetFullPath(args[0]);
var fixtures = Path.GetFullPath(args[1]);
if (Directory.Exists(fixtures) && Directory.EnumerateFileSystemEntries(fixtures).Any())
    throw new ArgumentException("Fixture directory must be empty.");
Directory.CreateDirectory(fixtures);
var cache = Path.Combine(fixtures, "cache");
var converter = new LocalMediaConverter(ffmpeg, cache);
var avi = Path.Combine(fixtures, "clip.avi");
var audio = Path.Combine(fixtures, "track.aiff");

async Task Run(params string[] arguments)
{
    var start = new ProcessStartInfo(ffmpeg) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true };
    foreach (var arg in new[] { "-nostdin", "-hide_banner", "-loglevel", "error", "-y" }.Concat(arguments))
        start.ArgumentList.Add(arg);
    using var process = Process.Start(start) ?? throw new IOException("FFmpeg did not start.");
    var errorTask = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    var error = await errorTask;
    if (process.ExitCode != 0) throw new Exception(error);
}

void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

await Run("-f", "lavfi", "-i", "testsrc2=size=160x90:rate=24", "-f", "lavfi", "-i", "sine=frequency=440:sample_rate=48000",
    "-t", "2", "-c:v", "mpeg4", "-q:v", "4", "-c:a", "pcm_s16le", avi);
await Run("-f", "lavfi", "-i", "sine=frequency=330:sample_rate=48000", "-t", "1", "-c:a", "pcm_s16be", audio);
var original = await File.ReadAllBytesAsync(avi);
var videoName = await converter.ConvertAsync(avi, "video", CancellationToken.None);
var audioName = await converter.ConvertAsync(audio, "audio", CancellationToken.None);
Check(videoName.EndsWith(".mp4") && audioName.EndsWith(".m4a"), "Incorrect conversion containers.");
await Run("-i", Path.Combine(cache, videoName), "-f", "null", "-");
await Run("-i", Path.Combine(cache, audioName), "-f", "null", "-");
Check(original.SequenceEqual(await File.ReadAllBytesAsync(avi)), "The original AVI was modified.");
var writtenAt = File.GetLastWriteTimeUtc(Path.Combine(cache, videoName));
Check(await converter.ConvertAsync(avi, "video", CancellationToken.None) == videoName, "Cache lookup changed the output name.");
Check(File.GetLastWriteTimeUtc(Path.Combine(cache, videoName)) == writtenAt, "A cache hit reconverted the source.");

var corrupt = Path.Combine(fixtures, "corrupt.avi");
await File.WriteAllBytesAsync(corrupt, new byte[] { 1, 2, 3, 4 });
try { await converter.ConvertAsync(corrupt, "video", CancellationToken.None); throw new Exception("Corrupt media was accepted."); }
catch (IOException error) { Check(error.Message.StartsWith("Local conversion failed:"), "Conversion failure was not explicit."); }
Check(!Directory.EnumerateFiles(cache, "*.partial.*").Any(), "Failed conversion left partial output.");

using var cancelled = new CancellationTokenSource();
cancelled.Cancel();
try { await converter.ConvertAsync(avi, "video", cancelled.Token); throw new Exception("Cancelled cache hit resumed playback."); }
catch (OperationCanceledException) { }
var cancelSource = Path.Combine(fixtures, "cancel.avi");
File.Copy(avi, cancelSource);
using var cancelRunning = new CancellationTokenSource(TimeSpan.FromMilliseconds(1));
try { await converter.ConvertAsync(cancelSource, "video", cancelRunning.Token); throw new Exception("Running conversion ignored cancellation."); }
catch (OperationCanceledException) { }
Check(!Directory.EnumerateFiles(cache, "*.partial.*").Any(), "Cancelled conversion left partial output.");

var missing = new LocalMediaConverter(Path.Combine(fixtures, "missing.exe"), cache);
try { await missing.ConvertAsync(avi, "video", CancellationToken.None); throw new Exception("Missing FFmpeg was ignored."); }
catch (FileNotFoundException error) { Check(error.Message.Contains("Bundled FFmpeg"), "Missing dependency error was unclear."); }
Console.WriteLine(JsonSerializer.Serialize(new {
    tests = 8, result = "passed", avi, audio, videoName, audioName, cache,
    originalPreserved = true, cancellation = "passed", corruptMedia = "reported"
}));
