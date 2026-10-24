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
        var args = "--autoplay-policy=no-user-gesture-required";
        var debugPort = Environment.GetEnvironmentVariable("AIWARP_DEBUG_PORT");
        if (int.TryParse(debugPort, out var port) && port > 0 && port < 65536)
            args += $" --remote-debugging-port={port}";
        var env = await CoreWebView2Environment.CreateAsync(null, profile, new CoreWebView2EnvironmentOptions(args));
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
        core.Navigate($"https://{Host}/index.html");
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
