using System.Runtime.InteropServices;

namespace AiWarpEngine;

// Per-process audio capture through the Windows audio driver (Process Loopback, Windows 10 build 19041+).
// Records only this app's process tree, so nothing else playing on the PC reaches the visuals.
internal sealed class ProcessLoopbackCapture : IDisposable
{
    private const int SampleRate = 48000;
    private const int Channels = 2;

    [ComImport, Guid("94ea2b94-e9cc-49e0-c0ff-ee64ca8f5b90"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAgileObject { }

    [ComImport, Guid("72A22D78-CDE4-431D-B8CC-843A71199B6D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IActivateAudioInterfaceAsyncOperation
    {
        void GetActivateResult(out int activateResult, [MarshalAs(UnmanagedType.IUnknown)] out object activatedInterface);
    }

    [ComImport, Guid("41D949AB-9862-444A-80F6-C261334DA5EB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IActivateAudioInterfaceCompletionHandler
    {
        void ActivateCompleted(IActivateAudioInterfaceAsyncOperation operation);
    }

    [ComImport, Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioClient
    {
        void Initialize(int shareMode, uint streamFlags, long bufferDuration, long periodicity, IntPtr format, IntPtr sessionGuid);
        void GetBufferSize(out uint frames);
        void GetStreamLatency(out long latency);
        void GetCurrentPadding(out uint frames);
        [PreserveSig] int IsFormatSupported(int shareMode, IntPtr format, out IntPtr closest);
        void GetMixFormat(out IntPtr format);
        void GetDevicePeriod(out long defaultPeriod, out long minimumPeriod);
        void Start();
        void Stop();
        void Reset();
        void SetEventHandle(IntPtr eventHandle);
        void GetService([MarshalAs(UnmanagedType.LPStruct)] Guid riid, [MarshalAs(UnmanagedType.IUnknown)] out object service);
    }

    [ComImport, Guid("C8ADBD64-E71E-48a0-A4DE-185C395CD317"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IAudioCaptureClient
    {
        void GetBuffer(out IntPtr data, out uint frames, out uint flags, out ulong devicePosition, out ulong qpcPosition);
        void ReleaseBuffer(uint frames);
        void GetNextPacketSize(out uint frames);
    }

    [DllImport("Mmdevapi.dll", ExactSpelling = true, PreserveSig = false)]
    private static extern void ActivateAudioInterfaceAsync(
        [MarshalAs(UnmanagedType.LPWStr)] string deviceInterfacePath,
        [MarshalAs(UnmanagedType.LPStruct)] Guid riid,
        IntPtr activationParams,
        IActivateAudioInterfaceCompletionHandler completionHandler,
        out IActivateAudioInterfaceAsyncOperation operation);

    private sealed class Handler : IActivateAudioInterfaceCompletionHandler, IAgileObject
    {
        public readonly ManualResetEventSlim Done = new(false);
        public int Result = unchecked((int)0x80004005);
        public object Client;

        public void ActivateCompleted(IActivateAudioInterfaceAsyncOperation operation)
        {
            try { operation.GetActivateResult(out Result, out Client); }
            catch { Result = unchecked((int)0x80004005); }
            Done.Set();
        }
    }

    private readonly Action<float[], int> onSamples;
    private IAudioClient client;
    private IAudioCaptureClient capture;
    private EventWaitHandle bufferEvent;
    private Thread thread;
    private volatile bool running;

    public ProcessLoopbackCapture(Action<float[], int> onSamples) { this.onSamples = onSamples; }

    public bool Start(int processId)
    {
        using var ready = new ManualResetEventSlim(false);
        bool ok = false;
        // COM objects from the activation callback live in the MTA, so the whole capture runs on one MTA thread.
        thread = new Thread(() =>
        {
            ok = Setup(processId);
            ready.Set();
            if (ok) Loop();
            Teardown();
        }) { IsBackground = true, Name = "AiWarp-ProcessLoopback" };
        thread.SetApartmentState(ApartmentState.MTA);
        thread.Start();
        if (!ready.Wait(6000)) { running = false; return false; }
        return ok;
    }

    private bool Setup(int processId)
    {
        IntPtr paramsPtr = IntPtr.Zero, propPtr = IntPtr.Zero, fmt = IntPtr.Zero;
        try
        {
            // AUDIOCLIENT_ACTIVATION_PARAMS { type = PROCESS_LOOPBACK(1); pid; mode = INCLUDE_TARGET_PROCESS_TREE(0) }
            paramsPtr = Marshal.AllocHGlobal(12);
            Marshal.WriteInt32(paramsPtr, 0, 1);
            Marshal.WriteInt32(paramsPtr, 4, processId);
            Marshal.WriteInt32(paramsPtr, 8, 0);
            // PROPVARIANT: VT_BLOB (0x41) pointing at the params
            propPtr = Marshal.AllocHGlobal(24);
            for (int i = 0; i < 24; i += 8) Marshal.WriteInt64(propPtr, i, 0);
            Marshal.WriteInt16(propPtr, 0, 0x41);
            Marshal.WriteInt32(propPtr, 8, 12);
            Marshal.WriteIntPtr(propPtr, 16, paramsPtr);

            var handler = new Handler();
            ActivateAudioInterfaceAsync("VAD\\Process_Loopback", typeof(IAudioClient).GUID, propPtr, handler, out _);
            if (!handler.Done.Wait(4000) || handler.Result < 0 || handler.Client is not IAudioClient ac) return false;
            client = ac;

            // WAVEFORMATEX: 32-bit float, stereo, 48 kHz (process loopback has no mix format to query)
            fmt = Marshal.AllocHGlobal(18);
            Marshal.WriteInt16(fmt, 0, 3);
            Marshal.WriteInt16(fmt, 2, Channels);
            Marshal.WriteInt32(fmt, 4, SampleRate);
            Marshal.WriteInt32(fmt, 8, SampleRate * Channels * 4);
            Marshal.WriteInt16(fmt, 12, Channels * 4);
            Marshal.WriteInt16(fmt, 14, 32);
            Marshal.WriteInt16(fmt, 16, 0);

            const uint flags = 0x00020000 | 0x00040000 | 0x80000000 | 0x08000000; // LOOPBACK | EVENTCALLBACK | AUTOCONVERTPCM | SRC_DEFAULT_QUALITY
            client.Initialize(0, flags, 2000000, 0, fmt, IntPtr.Zero);
            bufferEvent = new EventWaitHandle(false, EventResetMode.AutoReset);
            client.SetEventHandle(bufferEvent.SafeWaitHandle.DangerousGetHandle());
            client.GetService(typeof(IAudioCaptureClient).GUID, out var svc);
            capture = (IAudioCaptureClient)svc;
            client.Start();
            running = true;
            return true;
        }
        catch { return false; }
        finally
        {
            if (paramsPtr != IntPtr.Zero) Marshal.FreeHGlobal(paramsPtr);
            if (propPtr != IntPtr.Zero) Marshal.FreeHGlobal(propPtr);
            if (fmt != IntPtr.Zero) Marshal.FreeHGlobal(fmt);
        }
    }

    private void Loop()
    {
        var buf = new float[8192 * Channels];
        while (running)
        {
            try
            {
                if (!bufferEvent.WaitOne(100)) continue;
                capture.GetNextPacketSize(out var packet);
                while (packet > 0 && running)
                {
                    capture.GetBuffer(out var data, out var frames, out var flags, out _, out _);
                    int count = (int)Math.Min(frames, 8192u) * Channels;
                    if ((flags & 2) != 0) Array.Clear(buf, 0, count); // AUDCLNT_BUFFERFLAGS_SILENT
                    else Marshal.Copy(data, buf, 0, count);
                    capture.ReleaseBuffer(frames);
                    onSamples(buf, count / Channels);
                    capture.GetNextPacketSize(out packet);
                }
            }
            catch { break; }
        }
    }

    public void Dispose()
    {
        running = false;
        try { thread?.Join(1500); } catch { }
    }

    private void Teardown()
    {
        try { client?.Stop(); } catch { }
        if (capture is not null) { try { Marshal.ReleaseComObject(capture); } catch { } capture = null; }
        if (client is not null) { try { Marshal.ReleaseComObject(client); } catch { } client = null; }
        bufferEvent?.Dispose(); bufferEvent = null;
    }
}
