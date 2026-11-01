using AiWarpEngine;
using System.Text.Json;

if (args.Length != 1) throw new ArgumentException("Supply exactly one argument: an empty fixture directory. Fixtures are generated and checked with Windows media components only.");
var fixtures = Path.GetFullPath(args[0]);
if (Directory.Exists(fixtures) && Directory.EnumerateFileSystemEntries(fixtures).Any())
    throw new ArgumentException("Fixture directory must be empty.");
Directory.CreateDirectory(fixtures);
var cache = Path.Combine(fixtures, "cache");
var converter = new LocalMediaConverter(cache);

void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

var video = Path.Combine(fixtures, "clip.mp4");
var audio = Path.Combine(fixtures, "track.wav");
var unsupported = Path.Combine(fixtures, "unsupported.avi");
var avi = Path.Combine(fixtures, "mjpeg.avi");
var rawAvi = Path.Combine(fixtures, "raw.avi");
var silent = Path.Combine(fixtures, "silent.mp4");
await NativeFixtures.WriteMp4Async(video, 2, 440);
await File.WriteAllBytesAsync(audio, NativeFixtures.Wav(2, 330));
await File.WriteAllBytesAsync(unsupported, NativeFixtures.UnsupportedCodecAvi("FFV1", 1));
await File.WriteAllBytesAsync(avi, await NativeFixtures.MjpegAviAsync(1, 440));
await File.WriteAllBytesAsync(rawAvi, NativeFixtures.RawAvi(1, 440));
await NativeFixtures.WriteMp4Async(silent, 1, null);
await NativeDecode.VideoAsync(video, 2, 440);
await NativeDecode.AudioAsync(audio, 2, 330);
await NativeDecode.VideoAsync(silent, 1, null);
var original = await File.ReadAllBytesAsync(video);
var videoName = await converter.ConvertAsync(video, "video", CancellationToken.None);
var audioName = await converter.ConvertAsync(audio, "audio", CancellationToken.None);
Check(videoName.EndsWith(".mp4") && audioName.EndsWith(".m4a"), "Incorrect containers.");
await NativeDecode.VideoAsync(Path.Combine(cache, videoName), 2, 440);
await NativeDecode.AudioAsync(Path.Combine(cache, audioName), 2, 330);
string aviName = null;
try { aviName = await converter.ConvertAsync(avi, "video", CancellationToken.None); }
catch (IOException error) { Check(error.Message.Contains("Windows"), "AVI failure was not explicit."); }
if (aviName is not null) await NativeDecode.VideoAsync(Path.Combine(cache, aviName), 1, null);
// Positive control: an uncompressed AVI needs no video codec, so it must convert and play, audio included.
var rawAviName = await converter.ConvertAsync(rawAvi, "video", CancellationToken.None);
Check(rawAviName.EndsWith(".mp4"), "Uncompressed AVI did not convert to MP4.");
await NativeDecode.VideoAsync(Path.Combine(cache, rawAviName), 1, 440);
var silentName = await converter.ConvertAsync(silent, "video", CancellationToken.None);
await NativeDecode.VideoAsync(Path.Combine(cache, silentName), 1, null);
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
    videoName, audioName, aviName, rawAviName, silentName, cache, originalPreserved = true, unsupportedCodec = "explicit error", cancellation = "passed" }));
