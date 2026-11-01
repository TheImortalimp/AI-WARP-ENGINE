using AiWarpEngine;
using System.Diagnostics;
using System.Text.Json;

if (args.Length != 2) throw new ArgumentException("Supply a fixture-generation FFmpeg executable and an empty fixture directory. FFmpeg is test-only, not shipped.");
var generator = Path.GetFullPath(args[0]);
var fixtures = Path.GetFullPath(args[1]);
if (Directory.Exists(fixtures) && Directory.EnumerateFileSystemEntries(fixtures).Any())
    throw new ArgumentException("Fixture directory must be empty.");
Directory.CreateDirectory(fixtures);
var cache = Path.Combine(fixtures, "cache");
var converter = new LocalMediaConverter(cache);

async Task Generate(params string[] arguments)
{
    var start = new ProcessStartInfo(generator) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardError = true };
    foreach (var arg in new[] { "-nostdin", "-hide_banner", "-loglevel", "error", "-y" }.Concat(arguments))
        start.ArgumentList.Add(arg);
    using var process = Process.Start(start) ?? throw new IOException("Fixture generator did not start.");
    var errors = process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();
    if (process.ExitCode != 0) throw new Exception(await errors);
}

void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var video = Path.Combine(fixtures, "clip.mp4");
var audio = Path.Combine(fixtures, "track.wav");
var unsupported = Path.Combine(fixtures, "unsupported.avi");
var avi = Path.Combine(fixtures, "mjpeg.avi");
var silent = Path.Combine(fixtures, "silent.mp4");
await Generate("-f", "lavfi", "-i", "testsrc2=size=160x90:rate=24", "-f", "lavfi", "-i",
    "sine=frequency=440:sample_rate=48000", "-t", "2", "-c:v", "libx264", "-pix_fmt", "yuv420p", "-c:a", "aac", video);
await Generate("-f", "lavfi", "-i", "sine=frequency=330:sample_rate=48000", "-t", "2", "-c:a", "pcm_s16le", audio);
await Generate("-f", "lavfi", "-i", "testsrc2=size=160x90:rate=24", "-t", "1", "-c:v", "ffv1", unsupported);
await Generate("-f", "lavfi", "-i", "testsrc2=size=160x90:rate=24", "-f", "lavfi", "-i",
    "sine=frequency=440:sample_rate=48000", "-t", "1", "-c:v", "mjpeg", "-c:a", "pcm_s16le", avi);
await Generate("-f", "lavfi", "-i", "testsrc2=size=160x90:rate=24", "-t", "1", "-c:v", "libx264", "-pix_fmt", "yuv420p", silent);
var original = await File.ReadAllBytesAsync(video);
var videoName = await converter.ConvertAsync(video, "video", CancellationToken.None);
var audioName = await converter.ConvertAsync(audio, "audio", CancellationToken.None);
Check(videoName.EndsWith(".mp4") && audioName.EndsWith(".m4a"), "Incorrect containers.");
await Generate("-i", Path.Combine(cache, videoName), "-f", "null", "-");
await Generate("-i", Path.Combine(cache, audioName), "-f", "null", "-");
string aviName = null;
try
{
    aviName = await converter.ConvertAsync(avi, "video", CancellationToken.None);
    await Generate("-i", Path.Combine(cache, aviName), "-f", "null", "-");
}
catch (IOException error) { Check(error.Message.Contains("Windows"), "AVI failure was not explicit."); }
var silentName = await converter.ConvertAsync(silent, "video", CancellationToken.None);
await Generate("-i", Path.Combine(cache, silentName), "-f", "null", "-");
Check(original.SequenceEqual(await File.ReadAllBytesAsync(video)), "Original changed.");
var timestamp = File.GetLastWriteTimeUtc(Path.Combine(cache, videoName));
Check(await converter.ConvertAsync(video, "video", CancellationToken.None) == videoName, "Cache name changed.");
Check(File.GetLastWriteTimeUtc(Path.Combine(cache, videoName)) == timestamp, "Cache hit reconverted.");
try { await converter.ConvertAsync(unsupported, "video", CancellationToken.None); throw new Exception("Unsupported codec unexpectedly accepted."); }
catch (IOException error) { Check(error.Message.Contains("Windows"), "Unsupported-codec error was not explicit."); }
var corrupt = Path.Combine(fixtures, "corrupt.avi");
await File.WriteAllBytesAsync(corrupt, new byte[] { 1, 2, 3, 4 });
try { await converter.ConvertAsync(corrupt, "video", CancellationToken.None); throw new Exception("Corrupt file accepted."); }
catch (IOException error) { Check(error.Message.Contains("Windows"), "Corrupt-file error unclear."); }
using var cancelled = new CancellationTokenSource();
cancelled.Cancel();
try { await converter.ConvertAsync(video, "video", cancelled.Token); throw new Exception("Cancelled cache hit resumed."); }
catch (OperationCanceledException) { }
var cancelSource = Path.Combine(fixtures, "cancel.mp4");
File.Copy(video, cancelSource);
using var activeCancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(5));
try { await converter.ConvertAsync(cancelSource, "video", activeCancellation.Token); throw new Exception("Active conversion ignored cancellation."); }
catch (OperationCanceledException) { }
Check(!Directory.EnumerateFiles(cache, "*.partial.*").Any(), "Partial file left behind.");
try { await converter.ConvertAsync(video, "invalid", CancellationToken.None); throw new Exception("Invalid kind accepted."); }
catch (ArgumentException) { }
Console.WriteLine(JsonSerializer.Serialize(new { tests = 11, result = "passed", backend = "Windows MediaTranscoder",
    videoName, audioName, aviName, silentName, cache, originalPreserved = true, unsupportedCodec = "explicit error", cancellation = "passed" }));
