using System.Buffers.Binary;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Graphics.Imaging;
using Windows.Media.Editing;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Storage.Streams;

// Proves with Windows' own decoders that a file really plays, so no external decoder is needed.
internal static class NativeDecode
{
    private const double MaximumFrameError = 20;

    // Reads the container, then decodes frames across the clip and compares them with the synthetic picture
    // the fixture was made from. A tone, when given, is also decoded from the audio track.
    public static async Task VideoAsync(string path, double seconds, double? tone)
    {
        using var limit = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var name = Path.GetFileName(path);
        var file = await StorageFile.GetFileFromPathAsync(path).AsTask(limit.Token);
        var properties = await file.Properties.GetVideoPropertiesAsync().AsTask(limit.Token);
        Require(properties.Width == NativeFixtures.Width && properties.Height == NativeFixtures.Height,
            $"{name}: Windows reads a {properties.Width}x{properties.Height} picture.");
        Require(Math.Abs(properties.Duration.TotalSeconds - seconds) < 0.5, $"{name}: Windows reads a duration of {properties.Duration}.");
        var clip = await MediaClip.CreateFromFileAsync(file).AsTask(limit.Token);
        Require(clip.GetVideoEncodingProperties().Subtype == "H264", $"{name}: the video is not H.264.");
        if (tone is not null) Require(clip.EmbeddedAudioTracks.Count > 0, $"{name}: the video has no audio track.");
        var composition = new MediaComposition();
        composition.Clips.Add(clip);
        var count = (int)(seconds * NativeFixtures.FramesPerSecond);
        foreach (var index in new[] { 0, count / 4, count / 2, count * 3 / 4 })
        {
            var time = TimeSpan.FromTicks(index * TimeSpan.TicksPerSecond / NativeFixtures.FramesPerSecond);
            using var thumbnail = await composition.GetThumbnailAsync(time, NativeFixtures.Width, NativeFixtures.Height,
                VideoFramePrecision.NearestFrame).AsTask(limit.Token);
            var decoder = await BitmapDecoder.CreateAsync(thumbnail).AsTask(limit.Token);
            var data = await decoder.GetPixelDataAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, new BitmapTransform(),
                ExifOrientationMode.IgnoreExifOrientation, ColorManagementMode.DoNotColorManage).AsTask(limit.Token);
            var error = LumaError(data.DetachPixelData(), NativeFixtures.Frame(index, count));
            Require(error < MaximumFrameError, $"{name}: Windows decoded frame {index} with a luma error of {error:0.0}.");
        }
        if (tone is { } frequency) await AudioAsync(path, seconds, frequency);
    }

    // Decodes the whole audio stream to PCM with Windows and checks length, level and pitch of the sine tone.
    public static async Task AudioAsync(string path, double seconds, double tone)
    {
        using var limit = new CancellationTokenSource(TimeSpan.FromSeconds(90));
        var name = Path.GetFileName(path);
        var file = await StorageFile.GetFileFromPathAsync(path).AsTask(limit.Token);
        var music = await file.Properties.GetMusicPropertiesAsync().AsTask(limit.Token);
        Require(Math.Abs(music.Duration.TotalSeconds - seconds) < 0.5, $"{name}: Windows reads an audio duration of {music.Duration}.");
        using var input = await file.OpenReadAsync().AsTask(limit.Token);
        using var output = new InMemoryRandomAccessStream();
        var profile = MediaEncodingProfile.CreateWav(AudioEncodingQuality.High);
        profile.Audio.SampleRate = NativeFixtures.SampleRate;
        profile.Audio.ChannelCount = 2;
        profile.Audio.BitsPerSample = 16;
        var prepared = await new MediaTranscoder { AlwaysReencode = true }.PrepareStreamTranscodeAsync(input, output, profile)
            .AsTask(limit.Token);
        Require(prepared.CanTranscode, $"{name}: Windows cannot decode the audio ({prepared.FailureReason}).");
        await prepared.TranscodeAsync().AsTask(limit.Token);
        var wav = new byte[output.Size];
        output.Seek(0);
        await output.ReadAsync(wav.AsBuffer(), (uint)wav.Length, InputStreamOptions.None).AsTask(limit.Token);

        var (channels, rate, pcm) = ReadWav(wav, name);
        var frames = pcm.Length / (channels * 2);
        Require(Math.Abs(frames / (double)rate - seconds) < 0.25, $"{name}: Windows decoded {frames / (double)rate:0.000} s of audio.");
        // The middle of the signal avoids the encoder's start-up and tail.
        int first = frames / 5, last = frames * 4 / 5;
        for (var channel = 0; channel < channels; channel++)
        {
            double energy = 0;
            var crossings = 0;
            short previous = 0;
            for (var index = first; index < last; index++)
            {
                var sample = BinaryPrimitives.ReadInt16LittleEndian(pcm.AsSpan((index * channels + channel) * 2));
                energy += (double)sample * sample;
                if (index > first && (previous < 0) != (sample < 0)) crossings++;
                previous = sample;
            }
            var level = Math.Sqrt(energy / (last - first)) / short.MaxValue;
            var pitch = crossings / 2.0 / ((last - first) / (double)rate);
            Require(level is > 0.25 and < 0.45, $"{name}: decoded channel {channel} has level {level:0.000} instead of about 0.354.");
            Require(Math.Abs(pitch - tone) < tone * 0.03, $"{name}: decoded channel {channel} has pitch {pitch:0.0} Hz instead of {tone} Hz.");
        }
    }

    private static (int Channels, int Rate, byte[] Pcm) ReadWav(byte[] wav, string name)
    {
        Require(wav.Length > 12 && Encoding.ASCII.GetString(wav, 0, 4) == "RIFF" && Encoding.ASCII.GetString(wav, 8, 4) == "WAVE",
            $"{name}: Windows produced no PCM audio.");
        int channels = 0, rate = 0;
        for (var offset = 12; offset + 8 <= wav.Length;)
        {
            var id = Encoding.ASCII.GetString(wav, offset, 4);
            var size = BinaryPrimitives.ReadInt32LittleEndian(wav.AsSpan(offset + 4));
            var body = offset + 8;
            if (id == "fmt ")
            {
                channels = BinaryPrimitives.ReadInt16LittleEndian(wav.AsSpan(body + 2));
                rate = BinaryPrimitives.ReadInt32LittleEndian(wav.AsSpan(body + 4));
                Require(BinaryPrimitives.ReadInt16LittleEndian(wav.AsSpan(body + 14)) == 16, $"{name}: decoded audio is not 16-bit.");
            }
            else if (id == "data")
            {
                Require(channels > 0 && rate > 0, $"{name}: decoded audio has no format.");
                // A streamed WAV can leave the chunk size unpatched, so fall back to the rest of the stream.
                var length = size > 0 && size <= wav.Length - body ? size : wav.Length - body;
                return (channels, rate, wav.AsSpan(body, length).ToArray());
            }
            offset = body + size + (size & 1);
        }
        throw new InvalidOperationException($"{name}: Windows produced no PCM audio.");
    }

    private static double LumaError(byte[] actual, byte[] expected)
    {
        Require(actual.Length == expected.Length, "Windows returned a frame of the wrong size.");
        double total = 0;
        for (var offset = 0; offset < actual.Length; offset += 4) total += Math.Abs(Luma(actual, offset) - Luma(expected, offset));
        return total / (actual.Length / 4);
    }

    private static double Luma(byte[] bgra, int offset) => 0.114 * bgra[offset] + 0.587 * bgra[offset + 1] + 0.299 * bgra[offset + 2];

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
