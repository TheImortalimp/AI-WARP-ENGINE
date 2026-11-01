using AiWarpEngine;

var assertions = 0;
void Check(bool actual, bool expected, string message)
{
    assertions++;
    if (actual != expected) throw new Exception(message);
}
foreach (var path in new[] { "/index.html", "/ai_warp_engine.html", "/media_page.html" })
{
    Check(AppControlPolicy.IsAppDocument("https://warp.local" + path), true, path);
    Check(AppControlPolicy.IsAppDocument("https://warp.local" + path, lens: true), false, "LENS cannot become app");
}
foreach (var url in new[] {
    "http://warp.local/index.html", "https://warp.local:444/index.html",
    "https://user@warp.local/index.html", "https://warp.local.evil.invalid/index.html",
    "https://media.warp.local/index.html", "https://warp.local/yt_lens.html",
    "https://warp.local/untrusted.html", "file:///C:/app/index.html",
    "javascript:alert(1)", "https://www.youtube.com/", "about:blank"
})
    Check(AppControlPolicy.IsAppDocument(url), false, url);
Check(AppControlPolicy.IsAppDocument("https://warp.local/yt_lens.html?src=x", lens: true), true, "LENS player");
var names = new[] {
    "WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "WEBVIEW2_PIPE_FOR_SCRIPT_DEBUGGER",
    "WEBVIEW2_WAIT_FOR_SCRIPT_DEBUGGER"
};
var original = names.ToDictionary(name => name, Environment.GetEnvironmentVariable);
try
{
    foreach (var name in names) Environment.SetEnvironmentVariable(name, null);
    Check(AppControlPolicy.HasBrowserControlOverride(), false, "Normal release environment");
    foreach (var name in names)
    {
        Environment.SetEnvironmentVariable(name, "test-control-override");
        Check(AppControlPolicy.HasBrowserControlOverride(), true, name);
        Environment.SetEnvironmentVariable(name, null);
    }
}
finally
{
    foreach (var entry in original) Environment.SetEnvironmentVariable(entry.Key, entry.Value);
}
Console.WriteLine($"App control policy: {assertions} assertions passed.");
