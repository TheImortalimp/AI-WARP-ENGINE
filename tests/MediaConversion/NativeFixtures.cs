using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;

// Deterministic media fixtures built only from hand-written RIFF code and the codecs that ship with Windows.
internal static class NativeFixtures
{
    public const int Width = 160;
    public const int Height = 90;
    public const int FramesPerSecond = 24;
    public const int SampleRate = 48000;
    private const int SamplesPerFrame = SampleRate / FramesPerSecond;

    public static byte[] Wav(double seconds, double frequency)
    {
        var pcm = Pcm(seconds, frequency);
        using var memory = new MemoryStream();
        using var writer = new BinaryWriter(memory);
        writer.Write("RIFF"u8);
        writer.Write(36 + pcm.Length);
        writer.Write("WAVEfmt "u8);
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(SampleRate);
        writer.Write(SampleRate * 2);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8);
        writer.Write(pcm.Length);
        writer.Write(pcm);
        writer.Flush();
        return memory.ToArray();
    }

    // H.264 video (plus mono AAC audio when a tone is given) encoded by the Windows MediaTranscoder from raw NV12/PCM samples.
    public static async Task WriteMp4Async(string path, double seconds, double? tone)
    {
        var frames = (int)(seconds * FramesPerSecond);
        var pcm = tone is { } frequency ? Pcm(seconds, frequency) : null;
        var raw = VideoEncodingProperties.CreateUncompressed(MediaEncodingSubtypes.Nv12, Width, Height);
        raw.FrameRate.Numerator = FramesPerSecond;
        raw.FrameRate.Denominator = 1;
        raw.PixelAspectRatio.Numerator = 1;
        raw.PixelAspectRatio.Denominator = 1;
        var source = pcm is null
            ? new MediaStreamSource(new VideoStreamDescriptor(raw))
            : new MediaStreamSource(new VideoStreamDescriptor(raw),
                new AudioStreamDescriptor(AudioEncodingProperties.CreatePcm(SampleRate, 1, 16)));
        source.BufferTime = TimeSpan.Zero;
        source.Duration = TimeSpan.FromSeconds(frames / (double)FramesPerSecond);
        source.Starting += (_, e) => e.Request.SetActualStartPosition(TimeSpan.Zero);
        int nextFrame = 0, nextBlock = 0;
        source.SampleRequested += (_, e) =>
        {
            if (e.Request.StreamDescriptor is VideoStreamDescriptor)
            {
                if (nextFrame >= frames) return;
                var index = nextFrame++;
                var sample = MediaStreamSample.CreateFromBuffer(Nv12(Frame(index, frames)).AsBuffer(), Ticks(index, FramesPerSecond));
                sample.Duration = Ticks(index + 1, FramesPerSecond) - sample.Timestamp;
                e.Request.Sample = sample;
            }
            else
            {
                var offset = nextBlock * SamplesPerFrame * 2;
                if (offset >= pcm.Length) return;
                var chunk = pcm.AsSpan(offset, Math.Min(SamplesPerFrame * 2, pcm.Length - offset)).ToArray();
                var sample = MediaStreamSample.CreateFromBuffer(chunk.AsBuffer(), Ticks(nextBlock * SamplesPerFrame, SampleRate));
                sample.Duration = Ticks(nextBlock * SamplesPerFrame + chunk.Length / 2, SampleRate) - sample.Timestamp;
                nextBlock++;
                e.Request.Sample = sample;
            }
        };
        var profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Qvga);
        profile.Video.Width = Width;
        profile.Video.Height = Height;
        profile.Video.FrameRate.Numerator = FramesPerSecond;
        profile.Video.FrameRate.Denominator = 1;
        profile.Video.Bitrate = 1_000_000;
        if (pcm is null) profile.Audio = null;
        else
        {
            profile.Audio.SampleRate = SampleRate;
            profile.Audio.ChannelCount = 1;
            profile.Audio.Bitrate = 128_000;
        }
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(120));
        var folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(path)).AsTask(timeout.Token);
        var file = await folder.CreateFileAsync(Path.GetFileName(path), CreationCollisionOption.FailIfExists).AsTask(timeout.Token);
        using var output = await file.OpenAsync(FileAccessMode.ReadWrite).AsTask(timeout.Token);
        var transcoder = new MediaTranscoder { HardwareAccelerationEnabled = false };
        var prepared = await transcoder.PrepareMediaStreamSourceTranscodeAsync(source, output, profile).AsTask(timeout.Token);
        if (!prepared.CanTranscode)
            throw new InvalidOperationException($"Windows cannot create the H.264 test fixture ({prepared.FailureReason}).");
        await prepared.TranscodeAsync().AsTask(timeout.Token);
    }

    // AVI 1.0 with JPEG frames (MJPG) and 16-bit mono PCM audio.
    public static async Task<byte[]> MjpegAviAsync(double seconds, double tone)
    {
        var count = (int)(seconds * FramesPerSecond);
        var frames = new List<byte[]>();
        for (var index = 0; index < count; index++) frames.Add(await JpegAsync(Frame(index, count)));
        return Avi("MJPG", frames, Pcm(seconds, tone));
    }

    // AVI 1.0 with uncompressed bottom-up 24-bit frames (BI_RGB) and 16-bit mono PCM audio. It needs no video codec,
    // so Windows must convert it end to end.
    public static byte[] RawAvi(double seconds, double tone)
    {
        var count = (int)(seconds * FramesPerSecond);
        var frames = new List<byte[]>();
        for (var index = 0; index < count; index++)
        {
            var bgra = Frame(index, count);
            var dib = new byte[Width * Height * 3]; // 160 * 3 bytes per row is already DWORD aligned
            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            {
                var source = (y * Width + x) * 4;
                var target = ((Height - 1 - y) * Width + x) * 3;
                dib[target] = bgra[source];
                dib[target + 1] = bgra[source + 1];
                dib[target + 2] = bgra[source + 2];
            }
            frames.Add(dib);
        }
        return Avi("\0\0\0\0", frames, Pcm(seconds, tone));
    }

    // AVI whose video stream declares a codec Windows does not ship; the frame payloads are meaningless filler.
    public static byte[] UnsupportedCodecAvi(string codec, double seconds)
    {
        var frames = new List<byte[]>();
        for (var index = 0; index < (int)(seconds * FramesPerSecond); index++)
            frames.Add(Enumerable.Range(0, 2048).Select(offset => (byte)(index * 31 + offset * 7)).ToArray());
        return Avi(codec, frames, null);
    }

    private static TimeSpan Ticks(long position, int rate) => TimeSpan.FromTicks(position * TimeSpan.TicksPerSecond / rate);

    private static byte[] Pcm(double seconds, double frequency)
    {
        var bytes = new byte[(int)(SampleRate * seconds) * 2];
        for (var index = 0; index < bytes.Length / 2; index++)
        {
            var value = (short)Math.Round(Math.Sin(2 * Math.PI * frequency * index / SampleRate) * 0.5 * short.MaxValue);
            bytes[index * 2] = (byte)value;
            bytes[index * 2 + 1] = (byte)(value >> 8);
        }
        return bytes;
    }

    // Colour bars, a grey ramp, a moving box and a growing progress bar: every frame differs and none is black.
    public static byte[] Frame(int index, int count)
    {
        var pixels = new byte[Width * Height * 4];
        var bars = new[] { 0xEBEBEB, 0xEBEB10, 0x10EBEB, 0x10EB10, 0xEB10EB, 0xEB1010, 0x1010EB };
        var box = index * (Width - 24) / (count - 1);
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var rgb = y >= Height - 8 ? (x <= (index + 1) * Width / count ? 0xFFFF00 : 0x202020)
                : y >= Height * 2 / 3 ? 0x010101 * (x * 255 / (Width - 1))
                : bars[x * bars.Length / Width];
            if (x >= box && x < box + 24 && y >= 16 && y < 40) rgb = 0x101010;
            var offset = (y * Width + x) * 4;
            pixels[offset] = (byte)rgb;
            pixels[offset + 1] = (byte)(rgb >> 8);
            pixels[offset + 2] = (byte)(rgb >> 16);
            pixels[offset + 3] = 255;
        }
        return pixels;
    }

    // BT.601 limited-range NV12 so the encoder input is exactly what is written here.
    private static byte[] Nv12(byte[] bgra)
    {
        var nv12 = new byte[Width * Height * 3 / 2];
        for (var y = 0; y < Height; y++)
        for (var x = 0; x < Width; x++)
        {
            var offset = (y * Width + x) * 4;
            nv12[y * Width + x] = (byte)(16 + ((66 * bgra[offset + 2] + 129 * bgra[offset + 1] + 25 * bgra[offset] + 128) >> 8));
        }
        for (var y = 0; y < Height; y += 2)
        for (var x = 0; x < Width; x += 2)
        {
            int blue = 0, green = 0, red = 0;
            for (var corner = 0; corner < 4; corner++)
            {
                var offset = ((y + corner / 2) * Width + x + corner % 2) * 4;
                blue += bgra[offset];
                green += bgra[offset + 1];
                red += bgra[offset + 2];
            }
            blue /= 4;
            green /= 4;
            red /= 4;
            var target = Width * Height + y / 2 * Width + x;
            nv12[target] = (byte)(128 + ((-38 * red - 74 * green + 112 * blue + 128) >> 8));
            nv12[target + 1] = (byte)(128 + ((112 * red - 94 * green - 18 * blue + 128) >> 8));
        }
        return nv12;
    }

    private static async Task<byte[]> JpegAsync(byte[] bgra)
    {
        using var stream = new InMemoryRandomAccessStream();
        var quality = new BitmapPropertySet { ["ImageQuality"] = new BitmapTypedValue(0.9f, Windows.Foundation.PropertyType.Single) };
        var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream, quality);
        encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, Width, Height, 96, 96, bgra);
        await encoder.FlushAsync();
        var jpeg = new byte[stream.Size];
        stream.Seek(0);
        await stream.ReadAsync(jpeg.AsBuffer(), (uint)jpeg.Length, InputStreamOptions.None);
        return jpeg;
    }

    // RIFF AVI 1.0: hdrl (avih + one strl per stream), movi (00dc video / 01wb audio, interleaved) and idx1.
    private static byte[] Avi(string codec, List<byte[]> frames, byte[] pcm)
    {
        var audioBytes = SamplesPerFrame * 2;
        var largest = Math.Max(frames.Max(frame => frame.Length), pcm is null ? 0 : audioBytes);
        using var movi = new MemoryStream();
        movi.Write("movi"u8);
        var index = new List<(string Id, int Offset, int Length)>();
        void Add(string id, ReadOnlySpan<byte> data)
        {
            index.Add((id, (int)movi.Position, data.Length));
            movi.Write(Chunk(id, data));
        }
        for (var frame = 0; frame < frames.Count; frame++)
        {
            Add("00dc", frames[frame]);
            if (pcm is not null) Add("01wb", pcm.AsSpan(frame * audioBytes, audioBytes));
        }

        var streams = new List<byte[]>
        {
            Chunk("avih", Words(1_000_000 / FramesPerSecond, largest * FramesPerSecond, 0, 0x110, frames.Count, 0,
                pcm is null ? 1 : 2, largest, Width, Height, 0, 0, 0, 0)),
            Riff("LIST", "strl",
                Chunk("strh", StreamHeader("vids", codec, 1, FramesPerSecond, frames.Count, largest, 0, Width, Height)),
                Chunk("strf", Concat(Words(40, Width, Height), Shorts(1, 24), Ascii(codec), Words(Width * Height * 3, 0, 0, 0, 0))))
        };
        if (pcm is not null)
            streams.Add(Riff("LIST", "strl",
                Chunk("strh", StreamHeader("auds", "\0\0\0\0", 1, SampleRate, pcm.Length / 2, audioBytes, 2, 0, 0)),
                Chunk("strf", Concat(Shorts(1, 1), Words(SampleRate, SampleRate * 2), Shorts(2, 16)))));
        var entries = index.Select(entry => Concat(Ascii(entry.Id), Words(0x10, entry.Offset, entry.Length)));
        return Riff("RIFF", "AVI ", Riff("LIST", "hdrl", streams.ToArray()), Chunk("LIST", movi.ToArray()),
            Chunk("idx1", Concat(entries.ToArray())));
    }

    private static byte[] StreamHeader(string type, string handler, int scale, int rate, int length, int buffer,
        int sampleSize, int right, int bottom) =>
        Concat(Ascii(type), Ascii(handler), Words(0, 0, 0, scale, rate, 0, length, buffer, -1, sampleSize), Shorts(0, 0, right, bottom));

    private static byte[] Chunk(string id, ReadOnlySpan<byte> data)
    {
        var chunk = new byte[8 + data.Length + (data.Length & 1)];
        Ascii(id).CopyTo(chunk, 0);
        BitConverter.GetBytes(data.Length).CopyTo(chunk, 4);
        data.CopyTo(chunk.AsSpan(8));
        return chunk;
    }

    private static byte[] Riff(string id, string type, params byte[][] children) => Chunk(id, Concat(Ascii(type), Concat(children)));
    private static byte[] Ascii(string text) => Encoding.ASCII.GetBytes(text);
    private static byte[] Concat(params byte[][] parts) => parts.SelectMany(part => part).ToArray();
    private static byte[] Words(params int[] values) => values.SelectMany(value => BitConverter.GetBytes(value)).ToArray();
    private static byte[] Shorts(params int[] values) => values.SelectMany(value => BitConverter.GetBytes((short)value)).ToArray();
}
