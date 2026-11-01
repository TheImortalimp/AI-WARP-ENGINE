using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace AiWarpEngine;

internal sealed class LocalMediaConverter
{
    private readonly string executable;
    private readonly string cache;

    public LocalMediaConverter(string executable, string cache)
    {
        this.executable = executable;
        this.cache = cache;
        Directory.CreateDirectory(cache);
    }

    public async Task<string> ConvertAsync(string source, string kind, CancellationToken cancellation)
    {
        cancellation.ThrowIfCancellationRequested();
        if (kind != "video" && kind != "audio") throw new ArgumentException("Invalid media kind.");
        if (!File.Exists(executable)) throw new FileNotFoundException("Bundled FFmpeg is missing. Rebuild or reinstall AI WARP ENGINE.");
        var file = new FileInfo(source);
        if (!file.Exists) throw new FileNotFoundException("The selected media file no longer exists.");
        var identity = $"h264-stereo48k-aac-v2|{file.FullName}|{file.Length}|{file.LastWriteTimeUtc.Ticks}|{kind}";
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(identity))).ToLowerInvariant();
        var name = key + (kind == "video" ? ".mp4" : ".m4a");
        var target = Path.Combine(cache, name);
        if (File.Exists(target) && new FileInfo(target).Length > 0) return name;
        var temporary = Path.Combine(cache, Guid.NewGuid().ToString("N") + ".partial" + Path.GetExtension(name));
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardError = true
        };
        var args = new List<string> {
            "-nostdin", "-hide_banner", "-loglevel", "error", "-nostats", "-y",
            "-protocol_whitelist", "file,pipe", "-i", file.FullName
        };
        if (kind == "video")
        {
            args.AddRange(new[] {
                "-map", "0:v:0", "-map", "0:a:0?", "-c:v", "libx264", "-preset", "veryfast",
                "-crf", "20", "-vf", "pad=ceil(iw/2)*2:ceil(ih/2)*2", "-pix_fmt", "yuv420p"
            });
        }
        else args.AddRange(new[] { "-map", "0:a:0", "-vn" });
        args.AddRange(new[] { "-c:a", "aac", "-ac", "2", "-ar", "48000", "-b:a", "192k", "-movflags", "+faststart", temporary });
        foreach (var arg in args) start.ArgumentList.Add(arg);
        try
        {
            cancellation.ThrowIfCancellationRequested();
            using var process = new Process { StartInfo = start };
            if (!process.Start()) throw new IOException("FFmpeg could not start.");
            using var registration = cancellation.Register(() =>
            {
                try { if (!process.HasExited) process.Kill(entireProcessTree: true); }
                catch (InvalidOperationException) { /* The conversion exited before cancellation. */ }
            });
            var errorsTask = ReadErrorTailAsync(process.StandardError);
            await process.WaitForExitAsync();
            var errors = await errorsTask;
            cancellation.ThrowIfCancellationRequested();
            if (process.ExitCode != 0 || !File.Exists(temporary) || new FileInfo(temporary).Length == 0)
                throw new IOException("Local conversion failed: " + (string.IsNullOrWhiteSpace(errors)
                    ? "FFmpeg produced no playable output." : errors.Trim()));
            File.Move(temporary, target, overwrite: true);
            return name;
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }

    }

    private static async Task<string> ReadErrorTailAsync(StreamReader reader)
    {
        var tail = new StringBuilder();
        var buffer = new char[2048];
        int count;
        while ((count = await reader.ReadAsync(buffer.AsMemory())) > 0)
        {
            tail.Append(buffer, 0, count);
            if (tail.Length > 1200) tail.Remove(0, tail.Length - 1200);
        }
        return tail.ToString();
    }
}
