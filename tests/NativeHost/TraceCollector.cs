using System.Diagnostics;

namespace NativeHost;

internal sealed class HarnessException(string message) : Exception(message);

// Collects the app's own Trace output (for example blocked navigations) so it can travel with the result.
internal sealed class TraceCollector : TraceListener
{
    private const int MaxLines = 100;
    private const int MaxLineLength = 2000;
    private readonly List<string> lines = new();

    public void Add(string line)
    {
        lock (lines)
        {
            if (lines.Count < MaxLines) lines.Add(line.Length > MaxLineLength ? line[..MaxLineLength] : line);
        }
    }

    public IReadOnlyList<string> Snapshot()
    {
        lock (lines) return lines.ToArray();
    }

    public override void Write(string? message) { }

    public override void WriteLine(string? message) { }

    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? message) =>
        Add($"[{eventType}] {message}");

    public override void TraceEvent(TraceEventCache? eventCache, string source, TraceEventType eventType, int id, string? format, params object?[]? args) =>
        Add($"[{eventType}] {(args is { Length: > 0 } ? string.Format(format ?? "", args) : format)}");
}
