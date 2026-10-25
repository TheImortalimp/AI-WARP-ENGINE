using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AiWarpEngine;

// Taps the system output (WASAPI loopback) while the YT LENS plays and reduces it to the same
// 512-bin byte spectrum / waveform an AnalyserNode produces, so the page can use it as real audio.
internal sealed class AudioTap : IDisposable
{
    private const int N = 1024;
    private readonly object gate = new();
    private readonly float[] ring = new float[N];
    private int pos;
    private readonly float[] smooth = new float[N / 2];
    private WasapiLoopbackCapture cap;
    private ProcessLoopbackCapture direct;
    private System.Windows.Forms.Timer timer;
    private readonly Action<string> send;

    // "direct" = this app's own audio via the driver's process loopback; "system" = whole-output fallback.
    public string Mode { get; private set; } = "none";

    public AudioTap(Action<string> send) { this.send = send; }

    public bool Start(int targetPid = 0)
    {
        direct = new ProcessLoopbackCapture(PushInterleaved);
        if (direct.Start(targetPid > 0 ? targetPid : Environment.ProcessId)) Mode = "direct";
        else
        {
            direct.Dispose(); direct = null;
            try
            {
                cap = new WasapiLoopbackCapture();
                cap.DataAvailable += OnData;
                cap.StartRecording();
                Mode = "system";
            }
            catch { cap?.Dispose(); cap = null; return false; }
        }
        timer = new System.Windows.Forms.Timer { Interval = 33 };
        timer.Tick += (_, _) => Pump();
        timer.Start();
        return true;
    }

    private void PushInterleaved(float[] data, int frames)
    {
        lock (gate)
        {
            for (int i = 0; i < frames; i++)
            {
                ring[pos] = (data[i * 2] + data[i * 2 + 1]) * 0.5f;
                pos = (pos + 1) % N;
            }
        }
    }

    private void OnData(object s, WaveInEventArgs e)
    {
        var fmt = cap.WaveFormat;
        int ch = Math.Max(1, fmt.Channels);
        int bytes = fmt.BitsPerSample / 8;
        int frames = e.BytesRecorded / (bytes * ch);
        lock (gate)
        {
            for (int i = 0; i < frames; i++)
            {
                float sum = 0;
                for (int c = 0; c < ch; c++)
                {
                    int o = (i * ch + c) * bytes;
                    sum += fmt.Encoding == WaveFormatEncoding.IeeeFloat || bytes == 4 && fmt.BitsPerSample == 32 && fmt.Encoding != WaveFormatEncoding.Pcm
                        ? BitConverter.ToSingle(e.Buffer, o)
                        : bytes == 2 ? BitConverter.ToInt16(e.Buffer, o) / 32768f : 0f;
                }
                ring[pos] = sum / ch;
                pos = (pos + 1) % N;
            }
        }
    }

    private void Pump()
    {
        var re = new float[N];
        var im = new float[N];
        var wave = new byte[N / 2];
        lock (gate)
        {
            for (int i = 0; i < N; i++)
            {
                float v = ring[(pos + i) % N];
                double w = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (N - 1)); // Hann
                re[i] = (float)(v * w);
                if (i >= N / 2) wave[i - N / 2] = (byte)Math.Clamp(128 + v * 128f, 0, 255);
            }
        }
        Fft(re, im);
        var freq = new byte[N / 2];
        for (int k = 0; k < N / 2; k++)
        {
            float mag = MathF.Sqrt(re[k] * re[k] + im[k] * im[k]) / N * 2f;
            smooth[k] = 0.78f * smooth[k] + 0.22f * mag;
            float db = 20f * MathF.Log10(smooth[k] + 1e-9f);
            freq[k] = (byte)Math.Clamp((db + 100f) / 70f * 255f, 0, 255); // AnalyserNode: -100..-30 dB
        }
        var all = new byte[N];
        Buffer.BlockCopy(freq, 0, all, 0, N / 2);
        Buffer.BlockCopy(wave, 0, all, N / 2, N / 2);
        send("lens:audio|" + Convert.ToBase64String(all));
    }

    private static void Fft(float[] re, float[] im)
    {
        int n = re.Length;
        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) != 0; bit >>= 1) j ^= bit;
            j ^= bit;
            if (i < j) { (re[i], re[j]) = (re[j], re[i]); (im[i], im[j]) = (im[j], im[i]); }
        }
        for (int len = 2; len <= n; len <<= 1)
        {
            double ang = -2 * Math.PI / len;
            float wr = (float)Math.Cos(ang), wi = (float)Math.Sin(ang);
            for (int i = 0; i < n; i += len)
            {
                float cr = 1, ci = 0;
                for (int k = 0; k < len / 2; k++)
                {
                    int a = i + k, b = i + k + len / 2;
                    float tr = re[b] * cr - im[b] * ci, ti = re[b] * ci + im[b] * cr;
                    re[b] = re[a] - tr; im[b] = im[a] - ti;
                    re[a] += tr; im[a] += ti;
                    float ncr = cr * wr - ci * wi; ci = cr * wi + ci * wr; cr = ncr;
                }
            }
        }
    }

    public void Dispose()
    {
        timer?.Stop(); timer?.Dispose(); timer = null;
        direct?.Dispose(); direct = null;
        try { cap?.StopRecording(); } catch { }
        cap?.Dispose(); cap = null;
    }
}
