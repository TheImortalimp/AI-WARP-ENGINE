using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.Json.Nodes;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace NativeHost;

// Runs the app's real MainForm (loaded by reflection from the publish folder) and drives its WebView2 page.
internal sealed class HostSession
{
    private const string AppAssemblyFile = "AI-Warp-Engine.dll";
    private const string FormTypeName = "AiWarpEngine.MainForm";
    private const string ResultPoll =
        "(()=>{try{return JSON.stringify(window.checkResult||null)}catch(e){return JSON.stringify({ok:false,error:'result not serializable: '+e.message})}})()";

    private static readonly string[] BlockedEnvironment =
    {
        "WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", "WEBVIEW2_PIPE_FOR_SCRIPT_DEBUGGER", "WEBVIEW2_WAIT_FOR_SCRIPT_DEBUGGER"
    };

    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan EngineTimeout = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan ScriptTimeout = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan ShotTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(250);

    private readonly Options options;
    private readonly Report report;
    private readonly TraceCollector trace = new();
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly TaskCompletionSource<string> fatal = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<CoreWebView2InitializationCompletedEventArgs> coreReady = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<CoreWebView2NavigationCompletedEventArgs> navigated = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Form form = null!;
    private WebView2 view = null!;
    private bool useResultApi = true;
    private volatile bool engineReady;
    private volatile bool browserReady;
    private volatile string phase = "startup";

    public HostSession(Options options, Report report)
    {
        this.options = options;
        this.report = report;
    }

    public int Run()
    {
        PrepareProfile();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
        {
            trace.Add("UI thread exception: " + e.Exception);
            if (!engineReady) fatal.TrySetResult("the app raised an unhandled exception during startup: " + e.Exception.Message);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            report.Fail("unhandled exception: " + e.ExceptionObject);
            Publish();
            NativeMethods.KillOwnedBrowsers();
            Environment.Exit(1);
        };
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            report.Fail("interrupted");
            fatal.TrySetResult("interrupted");
        };
        Trace.Listeners.Add(trace);

        form = CreateForm();
        ChooseScreen();
        form.Load += (_, _) => ApplyPlacement();
        form.Shown += async (_, _) => await RunChecksAsync();
        form.FormClosed += (_, _) => fatal.TrySetResult("the app window closed before the check completed");
        view.CoreWebView2InitializationCompleted += (_, e) => coreReady.TrySetResult(e);
        view.NavigationCompleted += (_, e) => navigated.TrySetResult(e);

        using var watchdog = StartWatchdog();
        using var dialogWatch = new System.Windows.Forms.Timer { Interval = 500 };
        dialogWatch.Tick += (_, _) => CloseBlockingDialog();
        dialogWatch.Start();

        Log("starting the app window");
        Application.Run(form);
        dialogWatch.Stop();

        if (!report.Decided) report.Fail(fatal.Task.IsCompletedSuccessfully ? fatal.Task.Result : "the app window closed before the check completed");
        if (!report.Written) Publish();
        ReleaseBrowser();
        return report.Passed ? 0 : 1;
    }

    private void PrepareProfile()
    {
        var blocked = BlockedEnvironment.Where(name => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name))).ToArray();
        if (blocked.Length > 0)
            throw new HarnessException("the app refuses to start while these WebView2 override variables are set: " + string.Join(", ", blocked));
        foreach (var name in Environment.GetEnvironmentVariables().Keys.Cast<string>())
            if (name.StartsWith("WEBVIEW2_", StringComparison.OrdinalIgnoreCase))
                report.Warn($"{name} is set and can change WebView2 behavior");

        Directory.CreateDirectory(options.Profile);
        if (options.Media is not null) CopyMedia(options.Media, Path.Combine(options.Profile, "media-cache"));
        Environment.SetEnvironmentVariable("AIWARP_PROFILE", options.Profile);
    }

    private void CopyMedia(string source, string cache)
    {
        Directory.CreateDirectory(cache);
        var copied = 0;
        if (File.Exists(source))
        {
            File.Copy(source, Path.Combine(cache, Path.GetFileName(source)), overwrite: true);
            copied = 1;
        }
        else
        {
            foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
            {
                var target = Path.Combine(cache, Path.GetRelativePath(source, file));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                File.Copy(file, target, overwrite: true);
                copied++;
            }
        }
        Log($"copied {copied} media file(s) into {cache}");
    }

    private Form CreateForm()
    {
        var appDir = options.App.TrimEnd('\\');
        // The app maps warp.local to <AppContext.BaseDirectory>\app, so the base directory must be the publish folder.
        AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", appDir + "\\");
        if (!string.Equals(AppContext.BaseDirectory, appDir + "\\", StringComparison.OrdinalIgnoreCase))
            throw new HarnessException($"AppContext.BaseDirectory could not be redirected to '{appDir}' (it is '{AppContext.BaseDirectory}')");

        // Dependencies that the harness does not ship (NAudio, WinRT projections, ...) come from the publish folder.
        AssemblyLoadContext.Default.Resolving += (context, name) =>
        {
            var candidate = Path.Combine(appDir, name.Name + ".dll");
            return name.Name is not null && File.Exists(candidate) ? context.LoadFromAssemblyPath(candidate) : null;
        };

        var assembly = Assembly.LoadFrom(Path.Combine(appDir, AppAssemblyFile));
        var type = assembly.GetType(FormTypeName, throwOnError: false)
            ?? throw new HarnessException($"{FormTypeName} was not found in {AppAssemblyFile}");
        var created = Activator.CreateInstance(type, nonPublic: true) as Form
            ?? throw new HarnessException($"{FormTypeName} is not a Form");

        const BindingFlags instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        var field = type.GetField("view", instance)
            ?? type.GetFields(instance).FirstOrDefault(f => typeof(Control).IsAssignableFrom(f.FieldType) && f.FieldType.Name == nameof(WebView2))
            ?? throw new HarnessException($"{FormTypeName} has no WebView2 field");
        view = field.GetValue(created) as WebView2
            ?? throw new HarnessException(
                $"the app's WebView2 type ({field.FieldType.Assembly.GetName()}) is not the one this harness loaded " +
                $"({typeof(WebView2).Assembly.GetName()}); pin the same Microsoft.Web.WebView2 version in NativeHost.csproj");
        Log($"AppContext.BaseDirectory = {AppContext.BaseDirectory}");
        return created;
    }

    // Picks a monitor before the window exists so it is created there with that monitor's DPI.
    // Preference: a monitor where the requested client size fits, lowest scale factor first (CSS px == device px).
    private void ChooseScreen()
    {
        var needed = options.ClientSize ?? form.ClientSize;
        var candidates = Screen.AllScreens.Select(screen =>
        {
            var dpi = NativeMethods.MonitorDpi(screen);
            var frame = NativeMethods.FrameSize(dpi);
            var fits = needed.Width + frame.Width <= screen.WorkingArea.Width && needed.Height + frame.Height <= screen.WorkingArea.Height;
            return (screen, dpi, fits);
        }).ToList();
        var fitting = candidates.Where(c => c.fits).ToList();
        var best = fitting.Count > 0
            ? fitting.OrderBy(c => c.dpi).ThenByDescending(c => c.screen.Primary).First()
            : candidates.OrderByDescending(c => (long)c.screen.WorkingArea.Width * c.screen.WorkingArea.Height).ThenBy(c => c.dpi).First();
        Log($"placing the window on {best.screen.DeviceName} ({best.dpi * 100 / 96}% scale, {best.screen.WorkingArea.Width}x{best.screen.WorkingArea.Height} work area)");
        form.StartPosition = FormStartPosition.Manual;
        form.Location = new Point(best.screen.WorkingArea.Left + 24, best.screen.WorkingArea.Top + 24);
    }

    // Runs after the app's own Load handler, which caps and re-centers the window itself.
    private void ApplyPlacement()
    {
        // The app's own Load handler may already have closed the window after a startup failure.
        if (form.IsDisposed || form.Disposing) return;
        form.WindowState = FormWindowState.Normal;
        var workArea = Screen.FromControl(form).WorkingArea;
        if (options.ClientSize is { } want)
        {
            var frame = form.Size - form.ClientSize;
            // Windows refuses windows larger than the screen unless the maximum size is raised.
            if (want.Width + frame.Width > workArea.Width || want.Height + frame.Height > workArea.Height)
                form.MaximumSize = new Size(want.Width + frame.Width, want.Height + frame.Height);
            form.ClientSize = want;
        }
        form.Location = new Point(
            Math.Max(workArea.Left, workArea.Left + (workArea.Width - form.Width) / 2),
            Math.Max(workArea.Top, workArea.Top + (workArea.Height - form.Height) / 2));
    }

    private async Task RunChecksAsync()
    {
        try
        {
            EnsureClientSize();
            await WaitForPageAsync();
            await WaitForEngineAsync();
            await MeasureAsync();
            await InjectScriptsAsync();
            await RunCallAsync();
        }
        catch (Exception exception)
        {
            report.Fail(exception is HarnessException or TimeoutException ? exception.Message : exception.ToString());
            await MeasureBestEffortAsync();
        }
        phase = "finishing";
        await CaptureShotAsync();
        Publish();
        form.Close();
    }

    // A failed run should still say how large the window was and what the pixel ratio was, when that can be read.
    private async Task MeasureBestEffortAsync()
    {
        try
        {
            if (!form.IsHandleCreated || form.IsDisposed) return;
            if (report.ClientSize is null)
            {
                var size = ActualClientSize();
                report.ClientSize = new[] { size.Width, size.Height };
            }
            if (report.DevicePixelRatio is null && view.CoreWebView2 is not null)
            {
                var ratio = await EvalAsync("window.devicePixelRatio", TimeSpan.FromSeconds(5), "pixel ratio");
                if (ratio is JsonValue number && number.TryGetValue<double>(out var dpr)) report.DevicePixelRatio = dpr;
            }
        }
        catch (Exception exception)
        {
            Log("measurement after the failure was not possible: " + exception.Message);
        }
    }

    private async Task WaitForPageAsync()
    {
        phase = "webview2 initialization";
        var init = await Await(coreReady.Task, StartupTimeout, "WebView2 initialization");
        if (!init.IsSuccess)
            throw new HarnessException("WebView2 initialization failed: " + (init.InitializationException?.Message ?? "unknown error"));
        var core = view.CoreWebView2;
        report.WebView2Version = core.Environment.BrowserVersionString;
        browserReady = true;
        core.ProcessFailed += OnProcessFailed;
        Log($"WebView2 {report.WebView2Version} ready (browser pid {core.BrowserProcessId})");

        phase = "navigation";
        var navigation = await Await(navigated.Task, StartupTimeout, "navigation to the app page");
        if (!navigation.IsSuccess) throw new HarnessException($"navigation failed: {navigation.WebErrorStatus}");
        Log("navigation completed: " + core.Source);
    }

    private async Task WaitForEngineAsync()
    {
        phase = "engine readiness";
        var wait = Stopwatch.StartNew();
        while (true)
        {
            var left = EngineTimeout - wait.Elapsed;
            try
            {
                var probe = await EvalAsync("typeof window.aiWarpEngine !== 'undefined' && window.aiWarpEngine !== null",
                    left > TimeSpan.FromSeconds(1) ? left : TimeSpan.FromSeconds(1), "engine probe");
                if (probe is JsonValue value && value.TryGetValue<bool>(out var ready) && ready) break;
            }
            catch (Exception exception) when (exception is TimeoutException or COMException or InvalidOperationException)
            {
                Log("engine probe: " + exception.Message);
            }
            if (wait.Elapsed >= EngineTimeout)
            {
                await DiagnoseAsync();
                throw new HarnessException("engine not ready");
            }
            await Task.Delay(PollInterval);
        }
        engineReady = true;
        report.StartupMs = clock.ElapsedMilliseconds;
        Log($"window.aiWarpEngine is defined after {report.StartupMs} ms");
    }

    private async Task DiagnoseAsync()
    {
        try
        {
            var text = await EvalAsync(
                "JSON.stringify({url:location.href,title:document.title,readyState:document.readyState," +
                "engine:typeof window.aiWarpEngine,bridge:typeof (window.chrome&&window.chrome.webview)})",
                TimeSpan.FromSeconds(5), "diagnostics");
            if (text is JsonValue value && value.TryGetValue<string>(out var json)) report.Diagnostics = JsonNode.Parse(json);
        }
        catch (Exception exception)
        {
            Log("diagnostics failed: " + exception.Message);
        }
    }

    // Measured before any test script runs, because tests may override devicePixelRatio.
    private async Task MeasureAsync()
    {
        phase = "measuring";
        await Task.Delay(100);
        if (options.ClientSize is { } want && ActualClientSize() != want)
        {
            form.ClientSize = want;
            await Task.Delay(400);
        }
        var actual = ActualClientSize();
        if (options.ClientSize is { } requested && actual != requested)
            report.Warn($"requested client size {requested.Width}x{requested.Height} but the window client area is {actual.Width}x{actual.Height}");
        var node = await EvalAsync("JSON.stringify([window.devicePixelRatio,window.innerWidth,window.innerHeight])", ScriptTimeout, "page metrics");
        var metrics = JsonNode.Parse(((JsonValue)node!).GetValue<string>())!.AsArray();
        report.DevicePixelRatio = metrics[0]!.GetValue<double>();
        report.Viewport = new[] { (int)Math.Round(metrics[1]!.GetValue<double>()), (int)Math.Round(metrics[2]!.GetValue<double>()) };
        report.ClientSize = new[] { actual.Width, actual.Height };
        Log($"client {report.ClientSize[0]}x{report.ClientSize[1]}, viewport {report.Viewport[0]}x{report.Viewport[1]}, dpr {report.DevicePixelRatio}");
        Log(Geometry());
    }

    private async Task InjectScriptsAsync()
    {
        foreach (var path in options.Scripts)
        {
            phase = "script " + Path.GetFileName(path);
            await EvalAsync(await File.ReadAllTextAsync(path), ScriptTimeout, "script " + Path.GetFileName(path));
            Log("injected " + path);
        }
    }

    private async Task RunCallAsync()
    {
        phase = "call";
        var call = options.Call.Trim().TrimEnd(';').Trim();
        var wrapper = "window.checkResult=null;(async()=>{try{window.checkResult={ok:true,value:await (\n" + call +
            "\n)}}catch(e){window.checkResult={ok:false,error:String(e&&e.stack||e)}}})()";
        var limit = TimeSpan.FromSeconds(options.TimeoutSeconds);
        var started = Stopwatch.StartNew();
        await EvalAsync(wrapper, ScriptTimeout, "check wrapper");

        while (true)
        {
            var remaining = limit - started.Elapsed;
            if (remaining <= TimeSpan.Zero) { report.Fail("timeout"); return; }
            JsonNode? polled;
            try { polled = await EvalAsync(ResultPoll, remaining, "result poll"); }
            catch (TimeoutException) { report.Fail("timeout"); return; }
            if (polled is JsonValue value && value.TryGetValue<string>(out var json) && !string.IsNullOrEmpty(json) && json != "null")
            {
                report.CallMs = started.ElapsedMilliseconds;
                var result = JsonNode.Parse(json)!.AsObject();
                if (result["ok"] is JsonValue ok && ok.TryGetValue<bool>(out var passed) && passed) report.Pass(result["value"]?.DeepClone());
                else report.Fail(result["error"]?.ToString() ?? "the check failed without an error message");
                return;
            }
            var pause = remaining < PollInterval ? remaining : PollInterval;
            await Task.Delay(pause);
        }
    }

    private async Task CaptureShotAsync()
    {
        if (options.Shot is null) return;
        try
        {
            using var png = new MemoryStream();
            await Await(view.CoreWebView2.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, png), ShotTimeout, "screenshot");
            var directory = Path.GetDirectoryName(options.Shot);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            await File.WriteAllBytesAsync(options.Shot, png.ToArray());
            report.Shot = options.Shot;
        }
        catch (Exception exception)
        {
            report.Warn("screenshot failed: " + exception.Message);
        }
    }

    // Evaluates a script in the page and returns the parsed JSON completion value. Script exceptions are reported when the runtime supports it.
    private async Task<JsonNode?> EvalAsync(string script, TimeSpan timeout, string what)
    {
        var core = view.CoreWebView2;
        string? json = null;
        var done = false;
        if (useResultApi)
        {
            try
            {
                var result = await Await(core.ExecuteScriptWithResultAsync(script), timeout, what);
                if (!result.Succeeded) throw new HarnessException($"{what} failed: {DescribeScriptException(result.Exception)}");
                json = result.ResultAsJson;
                done = true;
            }
            catch (Exception exception) when (exception is NotImplementedException or InvalidCastException)
            {
                useResultApi = false;
            }
        }
        if (!done) json = await Await(core.ExecuteScriptAsync(script), timeout, what);
        return string.IsNullOrWhiteSpace(json) ? null : JsonNode.Parse(json);
    }

    private static string DescribeScriptException(CoreWebView2ScriptException? exception)
    {
        if (exception is null) return "unknown script error";
        var text = exception.Message.StartsWith(exception.Name, StringComparison.Ordinal) ? exception.Message : $"{exception.Name}: {exception.Message}";
        return $"{text} (line {exception.LineNumber}, column {exception.ColumnNumber})";
    }

    // Waits for the task, but gives up on a timeout or when the app window dies underneath us.
    private async Task Await(Task task, TimeSpan timeout, string what)
    {
        using var cancel = new CancellationTokenSource();
        var finished = await Task.WhenAny(task, fatal.Task, Task.Delay(timeout, cancel.Token));
        cancel.Cancel();
        if (finished == task)
        {
            await task;
            return;
        }
        if (finished == fatal.Task) throw new HarnessException(await fatal.Task);
        throw new TimeoutException($"{what} timed out after {timeout.TotalSeconds:0.#} s");
    }

    private async Task<T> Await<T>(Task<T> task, TimeSpan timeout, string what)
    {
        await Await((Task)task, timeout, what);
        return await task;
    }

    private void EnsureClientSize()
    {
        if (options.ClientSize is { } want && ActualClientSize() != want) form.ClientSize = want;
    }

    private Size ActualClientSize() => form.IsHandleCreated ? NativeMethods.ClientRectSize(form.Handle) : form.ClientSize;

    private string Geometry()
    {
        var bounds = form.IsHandleCreated ? NativeMethods.WindowBounds(form.Handle) : form.Bounds;
        var work = Screen.FromControl(form).WorkingArea;
        return $"window {bounds.Width}x{bounds.Height} at {bounds.Left},{bounds.Top}; client {ActualClientSize().Width}x{ActualClientSize().Height}; " +
            $"winforms client {form.ClientSize.Width}x{form.ClientSize.Height}; webview {view.Width}x{view.Height}; state {form.WindowState}; work area {work.Width}x{work.Height}";
    }

    private void OnProcessFailed(object? sender, CoreWebView2ProcessFailedEventArgs e)
    {
        var text = $"WebView2 process failed: {e.ProcessFailedKind} ({e.Reason}, exit code {e.ExitCode})";
        trace.Add(text);
        if (e.ProcessFailedKind is CoreWebView2ProcessFailedKind.BrowserProcessExited or CoreWebView2ProcessFailedKind.RenderProcessExited)
            fatal.TrySetResult(text);
    }

    private void CloseBlockingDialog()
    {
        if (!form.IsHandleCreated || report.Decided) return;
        var dialog = NativeMethods.FindModalDialog(form.Handle);
        if (dialog == IntPtr.Zero) return;
        var text = NativeMethods.DescribeDialog(dialog);
        trace.Add("blocking dialog: " + text);
        fatal.TrySetResult("the app showed a blocking dialog: " + text);
        NativeMethods.CloseWindow(dialog);
    }

    private System.Threading.Timer StartWatchdog()
    {
        var budget = StartupTimeout + EngineTimeout + ScriptTimeout * (options.Scripts.Count + 1) +
            TimeSpan.FromSeconds(options.TimeoutSeconds) + TimeSpan.FromSeconds(60);
        return new System.Threading.Timer(_ =>
        {
            if (!report.Decided) report.Fail($"harness watchdog expired during '{phase}' after {budget.TotalSeconds:0} s");
            if (!report.Written) Publish();
            NativeMethods.KillOwnedBrowsers();
            Environment.Exit(report.Passed ? 0 : 1);
        }, null, budget, Timeout.InfiniteTimeSpan);
    }

    private void Publish()
    {
        report.HostTrace = trace.Snapshot();
        report.Write();
    }

    // Disposing the window shuts the browser down. Anything still running afterwards (including a browser whose
    // initialization never completed) is terminated, so no msedgewebview2 child outlives this process.
    private void ReleaseBrowser()
    {
        var patient = browserReady;
        var soft = TimeSpan.FromSeconds(patient ? 5 : 1);
        var hard = soft + soft;
        var waited = Stopwatch.StartNew();
        var collected = false;
        while (true)
        {
            var owned = NativeMethods.OwnedBrowserProcesses();
            if (owned.Count == 0) return;
            if (!collected && waited.Elapsed >= soft)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                collected = true;
            }
            if (waited.Elapsed >= hard)
            {
                Console.Error.WriteLine("warning: the WebView2 browser process outlived the window; terminating it");
                foreach (var process in owned) NativeMethods.KillTree(process);
                foreach (var process in owned)
                {
                    try { process.WaitForExit(5000); }
                    catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception) { }
                }
                return;
            }
            Thread.Sleep(200);
        }
    }

    private void Log(string message)
    {
        if (options.Verbose) Console.Error.WriteLine($"[{clock.Elapsed.TotalSeconds:0.000}s] {message}");
    }
}
