using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Text.Json;

namespace AiWarpEngine;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        CursorGuard.HardenProcess();
        ApplicationConfiguration.Initialize();
        CursorGuard.Start("AI-Warp-Engine");
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private const string Host = "warp.local";
    private readonly WebView2 view = new() { Dock = DockStyle.Fill };
    private FormBorderStyle savedStyle;
    private FormWindowState savedState;
    private LocalMediaConverter mediaConverter;
    private CancellationTokenSource mediaConversion;
    private string mediaRequestId;
    private bool freezeCaptureBusy;

    public MainForm()
    {
        Text = "AI WARP ENGINE";
        AutoScaleMode = AutoScaleMode.Dpi;
        Size = new Size(1360, 860);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Black;
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
        Controls.Add(view);
        Load += async (_, _) =>
        {
            var workArea = Screen.FromControl(this).WorkingArea;
            var width = Math.Min(Width, workArea.Width);
            var height = Math.Min(Height, workArea.Height);
            Bounds = new Rectangle(workArea.Left + (workArea.Width - width) / 2,
                workArea.Top + (workArea.Height - height) / 2, width, height);
            try { await InitAsync(); }
            catch (Exception error) when (error is InvalidOperationException or ArgumentException or System.Runtime.InteropServices.COMException)
            {
                System.Diagnostics.Trace.TraceError("App startup: {0}", error);
                MessageBox.Show(this, error.Message, "AI WARP startup blocked", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        };
    }

    private async Task InitAsync()
    {
        var profile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AI-Warp-Engine", "profile");
        var profileOverride = Environment.GetEnvironmentVariable("AIWARP_PROFILE");
        if (!string.IsNullOrWhiteSpace(profileOverride)) profile = profileOverride;
        var args = "--autoplay-policy=no-user-gesture-required --disable-features=CalculateNativeWinOcclusion --disable-backgrounding-occluded-windows --disable-renderer-backgrounding";
#if DEBUG
        var debugPort = Environment.GetEnvironmentVariable("AIWARP_DEBUG_PORT");
        if (int.TryParse(debugPort, out var port) && port > 0 && port < 65536)
            args += $" --remote-debugging-port={port}";
#else
        if (AppControlPolicy.HasBrowserControlOverride())
            throw new InvalidOperationException("Release playback does not allow WebView2 browser-argument or script-debugger environment overrides. Remove the WEBVIEW2_* debugging override and restart.");
#endif
        var env = await CoreWebView2Environment.CreateAsync(null, profile, new CoreWebView2EnvironmentOptions(args));
        lensEnv = env;
        await view.EnsureCoreWebView2Async(env);

        var core = view.CoreWebView2;
        await CursorGuard.AttachAsync(core);
        CursorGuard.HardenBrowser(core);
        core.SetVirtualHostNameToFolderMapping(Host,
            Path.Combine(AppContext.BaseDirectory, "app"), CoreWebView2HostResourceAccessKind.DenyCors);
        var mediaCache = Path.Combine(profile, "media-cache");
        mediaConverter = new LocalMediaConverter(mediaCache);
        core.SetVirtualHostNameToFolderMapping("media.warp.local", mediaCache, CoreWebView2HostResourceAccessKind.Allow);
        core.Settings.AreDevToolsEnabled = false;
        core.Settings.AreHostObjectsAllowed = false;
        core.Settings.AreBrowserAcceleratorKeysEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.DocumentTitleChanged += (_, _) => Text = string.IsNullOrWhiteSpace(core.DocumentTitle) ? "AI WARP ENGINE" : core.DocumentTitle;
        core.NavigationStarting += (_, e) =>
        {
            if (!AppControlPolicy.IsAppDocument(e.Uri))
            {
                e.Cancel = true;
                NotifyControlBlocked("External top-level navigation was blocked.");
                return;
            }
            mediaConversion?.Cancel();
        };
        core.ContainsFullScreenElementChanged += (_, _) => SetFullscreen(core.ContainsFullScreenElement);
        core.NewWindowRequested += (_, e) =>
        {
            e.Handled = true;
            NotifyControlBlocked("Opening external browsers or new windows is disabled.");
        };
        core.PermissionRequested += (_, e) =>
        {
            // Camera/mic/display capture are only auto-granted to the app's own origin.
            e.State = AppControlPolicy.IsAppDocument(e.Uri) &&
                e.PermissionKind is CoreWebView2PermissionKind.Camera or CoreWebView2PermissionKind.Microphone
                ? CoreWebView2PermissionState.Allow : CoreWebView2PermissionState.Deny;
        };
        core.WebMessageReceived += (_, e) =>
        {
            if (!AppControlPolicy.IsAppDocument(e.Source))
            {
                NotifyControlBlocked("A native bridge request from an untrusted document was blocked.");
                return;
            }
            string msg;
            try { msg = e.TryGetWebMessageAsString(); } catch { return; }
            if (msg.StartsWith("lens:open|", StringComparison.Ordinal)) OpenLens(msg.Substring(10));
            else if (msg == "lens:close") CloseLens();
            else if (msg == "lens:frames|start") { StartFramePump(); StartTap(); }
            else if (msg == "lens:frames|stop") { pumpGen++; StopTap(); }
            else if (msg.StartsWith("media:convert|", StringComparison.Ordinal)) ConvertLocalMedia(msg.Substring(14), e);
            else if (msg.StartsWith("media:cancel|", StringComparison.Ordinal) && msg.Substring(13) == mediaRequestId) mediaConversion?.Cancel();
            else if (msg.StartsWith("freeze:capture|", StringComparison.Ordinal)) CaptureFreezeFrame(msg.Substring(15));
        };
        core.Navigate($"https://{Host}/index.html");
    }

    private void NotifyControlBlocked(string message)
    {
        System.Diagnostics.Trace.TraceWarning("App control: {0}", message);
        if (!IsDisposed && view.CoreWebView2 is not null)
            view.CoreWebView2.PostWebMessageAsString("control:blocked|" + message);
    }

    private CoreWebView2Environment? lensEnv;
    private Form? lensForm;
    private WebView2? lensView;

    private async void CaptureFreezeFrame(string id)
    {
        var core = view.CoreWebView2;
        var document = core.Source;
        var ownsCapture = false;
        try
        {
            if (string.IsNullOrEmpty(id) || id.Length > 20 || !id.All(char.IsAsciiDigit))
                throw new ArgumentException("Invalid freeze-frame request ID.");
            if (freezeCaptureBusy) throw new InvalidOperationException("A screenshot capture is already in progress.");
            freezeCaptureBusy = true;
            ownsCapture = true;
            using var image = new MemoryStream();
            await core.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, image);
            if (!IsDisposed && core.Source == document)
                core.PostWebMessageAsString("freeze:result|" + JsonSerializer.Serialize(new { id, png = Convert.ToBase64String(image.ToArray()) }));
        }
        catch (Exception error) when (error is ArgumentException or InvalidOperationException or IOException or System.Runtime.InteropServices.COMException)
        {
            System.Diagnostics.Trace.TraceError("Freeze-frame capture: {0}", error);
            if (!IsDisposed && core.Source == document)
                core.PostWebMessageAsString("freeze:result|" + JsonSerializer.Serialize(new { id, error = error.Message }));
        }
        finally
        {
            if (ownsCapture) freezeCaptureBusy = false;
        }
    }

    private async void ConvertLocalMedia(string json, CoreWebView2WebMessageReceivedEventArgs message)
    {
        string id = null;
        CancellationTokenSource conversion = null;
        try
        {
            using var request = JsonDocument.Parse(json);
            id = request.RootElement.GetProperty("id").GetString();
            var kind = request.RootElement.GetProperty("kind").GetString();
            if (string.IsNullOrEmpty(id) || id.Length > 20 || !id.All(char.IsAsciiDigit))
                throw new ArgumentException("Invalid conversion request ID.");
            var objects = message.AdditionalObjects;
            if (objects.Count != 1 || objects[0] is not CoreWebView2File file)
                throw new ArgumentException("Conversion requires one user-selected local file.");
            mediaConversion?.Cancel();
            conversion = new CancellationTokenSource();
            mediaConversion = conversion;
            mediaRequestId = id;
            var name = await mediaConverter.ConvertAsync(file.Path, kind, conversion.Token);
            if (!conversion.IsCancellationRequested && !IsDisposed)
                view.CoreWebView2.PostWebMessageAsString("media:result|" + JsonSerializer.Serialize(new { id, url = "https://media.warp.local/" + name }));
        }
        catch (OperationCanceledException) { /* Source changes explicitly cancel pending conversion. */ }
        catch (Exception error) when (error is ArgumentException or JsonException or KeyNotFoundException or InvalidOperationException or IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception or NotImplementedException or System.Runtime.InteropServices.COMException)
        {
            System.Diagnostics.Trace.TraceError("Local media conversion: {0}", error);
            if (!IsDisposed)
                view.CoreWebView2.PostWebMessageAsString("media:result|" + JsonSerializer.Serialize(new { id, error = error.Message }));
        }
        finally
        {
            if (mediaConversion == conversion) { mediaConversion = null; mediaRequestId = null; }
            conversion?.Dispose();
        }
    }

    // The "AI WARP LENS" window plays the YouTube embed; the main page captures it (getDisplayMedia,
    // auto-selected by window title) so the canvas pipeline receives the real video pixels.
    private async void OpenLens(string embedUrl)
    {
        var ok = Uri.TryCreate(embedUrl, UriKind.Absolute, out var u) && u.Scheme == Uri.UriSchemeHttps &&
                 (u.Host == "www.youtube-nocookie.com" || u.Host == "www.youtube.com") &&
                 string.IsNullOrEmpty(u.UserInfo) && u.IsDefaultPort &&
                 (u.AbsolutePath == "/embed" || u.AbsolutePath.StartsWith("/embed/", StringComparison.Ordinal));
        if (!ok || lensEnv is null) return;
        var target = $"https://{Host}/yt_lens.html?src={Uri.EscapeDataString(embedUrl)}";
        if (lensForm is { IsDisposed: false } && lensView?.CoreWebView2 is not null)
        {
            lensView.CoreWebView2.Navigate(target);
            return;
        }
        lensView = new WebView2 { Dock = DockStyle.Fill };
        lensForm = new Form
        {
            Text = "AI WARP LENS",
            Size = new Size(960, 560),
            StartPosition = FormStartPosition.Manual,
            Location = new Point(Left + 40, Top + 40),
            BackColor = Color.Black,
            ShowInTaskbar = false
        };
        // Parked off-screen: it keeps rendering (and playing audio) for capture without covering the app.
        lensForm.Location = new Point(-12000, 200);
        try { lensForm.Icon = Icon; } catch { }
        lensForm.Controls.Add(lensView);
        lensForm.FormClosed += (_, _) =>
        {
            lensForm = null;
            try { view.CoreWebView2?.PostWebMessageAsString("lens:closed"); } catch { }
        };
        lensForm.Show();
        await lensView.EnsureCoreWebView2Async(lensEnv);
        await CursorGuard.AttachAsync(lensView.CoreWebView2);
        CursorGuard.HardenBrowser(lensView.CoreWebView2);
        lensView.CoreWebView2.Settings.AreDevToolsEnabled = false;
        lensView.CoreWebView2.Settings.AreHostObjectsAllowed = false;
        lensView.CoreWebView2.Settings.IsWebMessageEnabled = false;
        lensView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
        lensView.CoreWebView2.SetVirtualHostNameToFolderMapping(Host,
            Path.Combine(AppContext.BaseDirectory, "app"), CoreWebView2HostResourceAccessKind.DenyCors);
        lensView.CoreWebView2.Settings.IsStatusBarEnabled = false;
        lensView.CoreWebView2.NewWindowRequested += (_, e) => e.Handled = true;
        lensView.CoreWebView2.NavigationStarting += (_, e) =>
        {
            if (AppControlPolicy.IsAppDocument(e.Uri, lens: true)) return;
            e.Cancel = true;
            NotifyControlBlocked("External LENS navigation was blocked.");
        };
        lensView.CoreWebView2.PermissionRequested += (_, e) => e.State = CoreWebView2PermissionState.Deny;
        lensView.CoreWebView2.Navigate(target);
        Activate();
    }

    // WebView2 has no scriptable screen-capture picker, so the lens compositor output (including the
    // cross-origin YouTube iframe) is snapshotted and streamed to the main page as JPEG frames.
    private int pumpGen;

    // Each start begins a new generation, so a loop stuck on a closed lens window can never block the next one.
    private async void StartFramePump()
    {
        var gen = ++pumpGen;
        while (gen == pumpGen)
        {
            var t0 = Environment.TickCount64;
            var core = lensView?.CoreWebView2;
            if (core is not null && lensForm is { IsDisposed: false } && view.CoreWebView2 is not null)
            {
                try
                {
                    using var ms = new MemoryStream();
                    await core.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Jpeg, ms);
                    if (gen == pumpGen && ms.Length > 0)
                        view.CoreWebView2.PostWebMessageAsString("lens:frame|" + Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length));
                }
                catch { }
            }
            var wait = 50 - (int)(Environment.TickCount64 - t0);
            await Task.Delay(Math.Max(wait, 5));
        }
    }

    private AudioTap tap;

    private void StartTap()
    {
        StopTap();
        var t = new AudioTap(m => { try { view.CoreWebView2?.PostWebMessageAsString(m); } catch { } });
        int browserPid = 0;
        try { browserPid = (int)(view.CoreWebView2?.BrowserProcessId ?? 0); } catch { }
        if (t.Start(browserPid))
        {
            tap = t;
            try { view.CoreWebView2?.PostWebMessageAsString("lens:audiomode|" + t.Mode); } catch { }
        }
        else t.Dispose();
    }

    private void StopTap() { tap?.Dispose(); tap = null; }

    private void CloseLens()
    {
        pumpGen++;
        StopTap();
        if (lensForm is { IsDisposed: false }) lensForm.Close();
        lensForm = null;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        mediaConversion?.Cancel();
        CloseLens();
        base.OnFormClosed(e);
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.F11) { SetFullscreen(FormBorderStyle != FormBorderStyle.None); return true; }
        if (keyData == Keys.F5) { view.CoreWebView2?.Reload(); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void SetFullscreen(bool on)
    {
        if (on == (FormBorderStyle == FormBorderStyle.None)) return;
        if (on)
        {
            savedStyle = FormBorderStyle; savedState = WindowState;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;
            WindowState = FormWindowState.Maximized;
        }
        else
        {
            FormBorderStyle = savedStyle == FormBorderStyle.None ? FormBorderStyle.Sizable : savedStyle;
            WindowState = savedState;
        }
    }
}
