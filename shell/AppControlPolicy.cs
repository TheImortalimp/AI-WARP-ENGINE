namespace AiWarpEngine;

internal static class AppControlPolicy
{
    public static bool IsAppDocument(string value, bool lens = false)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps || uri.Host != "warp.local" ||
            !uri.IsDefaultPort || !string.IsNullOrEmpty(uri.UserInfo)) return false;
        if (lens) return uri.AbsolutePath == "/yt_lens.html";
        return uri.AbsolutePath is "/index.html" or "/ai_warp_engine.html" or "/media_page.html";
    }

    public static bool HasBrowserControlOverride()
    {
        return new[] {
            "WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS",
            "WEBVIEW2_PIPE_FOR_SCRIPT_DEBUGGER",
            "WEBVIEW2_WAIT_FOR_SCRIPT_DEBUGGER"
        }.Any(name => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)));
    }
}
