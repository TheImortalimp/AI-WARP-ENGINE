using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace AiWarpEngine;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private const string Host = "warp.local";
    private readonly WebView2 view = new() { Dock = DockStyle.Fill };
    private FormBorderStyle savedStyle;
    private FormWindowState savedState;

    public MainForm()
    {
        Text = "AI WARP ENGINE";
        AutoScaleMode = AutoScaleMode.Dpi;
        Size = new Size(1360, 860);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.Black;
        try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
        Controls.Add(view);
        Load += async (_, _) => await InitAsync();
    }

    private async Task InitAsync()
    {
        var profile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AI-Warp-Engine", "profile");
        var profileOverride = Environment.GetEnvironmentVariable("AIWARP_PROFILE");
        if (!string.IsNullOrWhiteSpace(profileOverride)) profile = profileOverride;
        var args = "--autoplay-policy=no-user-gesture-required --disable-features=CalculateNativeWinOcclusion --disable-backgrounding-occluded-windows --disable-renderer-backgrounding";
        var debugPort = Environment.GetEnvironmentVariable("AIWARP_DEBUG_PORT");
        if (int.TryParse(debugPort, out var port) && port > 0 && port < 65536)
            args += $" --remote-debugging-port={port}";
        var env = await CoreWebView2Environment.CreateAsync(null, profile, new CoreWebView2EnvironmentOptions(args));
        lensEnv = env;
        await view.EnsureCoreWebView2Async(env);

        var core = view.CoreWebView2;
        core.SetVirtualHostNameToFolderMapping(Host,
            Path.Combine(AppContext.BaseDirectory, "app"), CoreWebView2HostResourceAccessKind.Allow);
        core.Settings.AreDevToolsEnabled = false;
        core.Settings.IsStatusBarEnabled = false;
        core.DocumentTitleChanged += (_, _) => Text = string.IsNullOrWhiteSpace(core.DocumentTitle) ? "AI WARP ENGINE" : core.DocumentTitle;
        core.ContainsFullScreenElementChanged += (_, _) => SetFullscreen(core.ContainsFullScreenElement);
        core.NewWindowRequested += (_, e) =>
        {
            e.Handled = true;
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri) { UseShellExecute = true }); } catch { }
        };
        core.PermissionRequested += (_, e) =>
        {
            // Camera/mic/display capture are only auto-granted to the app's own origin.
            if (e.Uri.StartsWith($"https://{Host}/", StringComparison.OrdinalIgnoreCase))
                e.State = CoreWebView2PermissionState.Allow;
        };
        core.WebMessageReceived += (_, e) =>
        {
            if (!e.Source.StartsWith($"https://{Host}/", StringComparison.OrdinalIgnoreCase)) return;
            string msg;
            try { msg = e.TryGetWebMessageAsString(); } catch { return; }
            if (msg.StartsWith("lens:open|", StringComparison.Ordinal)) OpenLens(msg.Substring(10));
            else if (msg == "lens:close") CloseLens();
            else if (msg == "lens:frames|start") StartFramePump();
            else if (msg == "lens:frames|stop") pumpOn = false;
        };
        core.Navigate($"https://{Host}/index.html");
    }

    private CoreWebView2Environment? lensEnv;
    private Form? lensForm;
    private WebView2? lensView;

    // The "AI WARP LENS" window plays the YouTube embed; the main page captures it (getDisplayMedia,
    // auto-selected by window title) so the canvas pipeline receives the real video pixels.
    private async void OpenLens(string embedUrl)
    {
        var ok = Uri.TryCreate(embedUrl, UriKind.Absolute, out var u) && u.Scheme == Uri.UriSchemeHttps &&
                 (u.Host == "www.youtube-nocookie.com" || u.Host == "www.youtube.com") &&
                 u.AbsolutePath.StartsWith("/embed", StringComparison.Ordinal);
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
        lensView.CoreWebView2.Settings.AreDevToolsEnabled = false;
        lensView.CoreWebView2.SetVirtualHostNameToFolderMapping(Host,
            Path.Combine(AppContext.BaseDirectory, "app"), CoreWebView2HostResourceAccessKind.Allow);
        lensView.CoreWebView2.Settings.IsStatusBarEnabled = false;
        lensView.CoreWebView2.NewWindowRequested += (_, e) => e.Handled = true;
        lensView.CoreWebView2.Navigate(target);
        Activate();
    }

    // WebView2 has no scriptable screen-capture picker, so the lens compositor output (including the
    // cross-origin YouTube iframe) is snapshotted and streamed to the main page as JPEG frames.
    private volatile bool pumpOn;
    private bool pumping;

    private async void StartFramePump()
    {
        pumpOn = true;
        if (pumping) return;
        pumping = true;
        try
        {
            while (pumpOn)
            {
                var t0 = Environment.TickCount64;
                var core = lensView?.CoreWebView2;
                if (core is not null && lensForm is { IsDisposed: false } && view.CoreWebView2 is not null)
                {
                    try
                    {
                        using var ms = new MemoryStream();
                        await core.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Jpeg, ms);
                        if (pumpOn && ms.Length > 0)
                            view.CoreWebView2.PostWebMessageAsString("lens:frame|" + Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length));
                    }
                    catch { }
                }
                var wait = 50 - (int)(Environment.TickCount64 - t0);
                await Task.Delay(Math.Max(wait, 5));
            }
        }
        finally { pumping = false; }
    }

    private void CloseLens()
    {
        pumpOn = false;
        if (lensForm is { IsDisposed: false }) lensForm.Close();
        lensForm = null;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
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
