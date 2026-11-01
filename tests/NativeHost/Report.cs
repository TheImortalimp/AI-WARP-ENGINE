using System.Diagnostics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NativeHost;

// The single source of truth for the result file. The first Pass/Fail wins; Write is atomic and repeatable.
internal sealed class Report
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly object gate = new();
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private readonly List<string> warnings = new();
    private bool? ok;
    private JsonNode? value;
    private string? error;
    private int writes;

    public string? OutPath { get; set; }
    public string? WebView2Version { get; set; }
    public int[]? ClientSize { get; set; }
    public int[]? Viewport { get; set; }
    public double? DevicePixelRatio { get; set; }
    public long? StartupMs { get; set; }
    public long? CallMs { get; set; }
    public string? Shot { get; set; }
    public JsonNode? Diagnostics { get; set; }
    public IReadOnlyList<string> HostTrace { get; set; } = Array.Empty<string>();

    public long ElapsedMs => clock.ElapsedMilliseconds;
    public bool Decided { get { lock (gate) return ok.HasValue; } }
    public bool Passed { get { lock (gate) return ok == true; } }
    public bool Written { get { lock (gate) return writes > 0; } }
    public string? Error { get { lock (gate) return error; } }

    public void Pass(JsonNode? result)
    {
        lock (gate)
        {
            if (ok.HasValue) return;
            ok = true;
            value = result;
        }
    }

    public void Fail(string message)
    {
        lock (gate)
        {
            if (ok.HasValue) return;
            ok = false;
            error = message;
        }
    }

    public void Warn(string message)
    {
        lock (gate) warnings.Add(message);
        Console.Error.WriteLine("warning: " + message);
    }

    public void Write()
    {
        if (OutPath is null) return;
        try
        {
            string text;
            lock (gate)
            {
                var passed = ok == true;
                var json = new JsonObject
                {
                    ["ok"] = passed,
                    ["value"] = passed ? value?.DeepClone() : null,
                    ["error"] = passed ? null : error ?? "no result was produced",
                    ["elapsedMs"] = clock.ElapsedMilliseconds,
                    ["webview2Version"] = WebView2Version,
                    ["clientSize"] = ClientSize is null ? null : new JsonArray(ClientSize[0], ClientSize[1]),
                    ["devicePixelRatio"] = DevicePixelRatio,
                };
                if (Viewport is not null) json["viewport"] = new JsonArray(Viewport[0], Viewport[1]);
                if (StartupMs is not null) json["startupMs"] = StartupMs;
                if (CallMs is not null) json["callMs"] = CallMs;
                if (Shot is not null) json["shot"] = Shot;
                if (Diagnostics is not null) json["diagnostics"] = Diagnostics.DeepClone();
                if (warnings.Count > 0) json["warnings"] = new JsonArray(warnings.Select(w => (JsonNode?)w).ToArray());
                if (HostTrace.Count > 0) json["hostTrace"] = new JsonArray(HostTrace.Select(t => (JsonNode?)t).ToArray());
                text = json.ToJsonString(JsonOptions);
                writes++;
            }
            var directory = Path.GetDirectoryName(OutPath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            var temporary = OutPath + ".tmp";
            File.WriteAllText(temporary, text, new UTF8Encoding(false));
            File.Move(temporary, OutPath, overwrite: true);
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("could not write the result file: " + exception.Message);
        }
    }

    public string Summary()
    {
        lock (gate)
        {
            var size = ClientSize is null ? "?" : $"{ClientSize[0]}x{ClientSize[1]}";
            var head = ok == true ? "ok" : "FAILED";
            var tail = ok == true ? "" : $" - {error}";
            return $"NativeHost: {head} in {clock.ElapsedMilliseconds} ms (client {size}, dpr {DevicePixelRatio?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "?"}, webview2 {WebView2Version ?? "?"}) -> {OutPath}{tail}";
        }
    }
}
