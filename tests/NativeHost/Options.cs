using System.Globalization;

namespace NativeHost;

internal sealed class UsageException(string message) : Exception(message);

internal sealed class Options
{
    public const string Usage =
        "usage: dotnet run --project tests\\NativeHost -c Release -- --app <publishDir> --profile <profileDir> --out <result.json>\n" +
        "         --script <file.js> [--script <file2.js> ...] --call \"<JS expression>\"\n" +
        "         [--size 1338x804] [--timeout 180] [--media <dir>] [--shot <png>] [--verbose]";

    public string App { get; private set; } = "";
    public string Profile { get; private set; } = "";
    public string Out { get; private set; } = "";
    public string Call { get; private set; } = "";
    public List<string> Scripts { get; } = new();
    public Size? ClientSize { get; private set; }
    public int TimeoutSeconds { get; private set; } = 180;
    public string? Media { get; private set; }
    public string? Shot { get; private set; }
    public bool Verbose { get; private set; }

    // Returns null when help was requested.
    public static Options? Parse(string[] args)
    {
        var options = new Options();
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
                throw new UsageException($"unexpected argument '{arg}'");
            string name = arg, value = "";
            var hasInline = false;
            var equals = arg.IndexOf('=');
            if (equals > 0)
            {
                name = arg[..equals];
                value = arg[(equals + 1)..];
                hasInline = true;
            }
            switch (name)
            {
                case "--help":
                    return null;
                case "--verbose":
                    options.Verbose = true;
                    continue;
            }
            if (!hasInline)
            {
                if (i + 1 >= args.Length) throw new UsageException($"option {name} needs a value");
                value = args[++i];
            }
            switch (name)
            {
                case "--app": options.App = FullPath(value); break;
                case "--profile": options.Profile = FullPath(value); break;
                case "--out": options.Out = FullPath(value); break;
                case "--script": options.Scripts.Add(FullPath(value)); break;
                case "--call": options.Call = value; break;
                case "--media": options.Media = FullPath(value); break;
                case "--shot": options.Shot = FullPath(value); break;
                case "--size": options.ClientSize = ParseSize(value); break;
                case "--timeout":
                    if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) || seconds < 1)
                        throw new UsageException($"--timeout needs a positive number of seconds, got '{value}'");
                    options.TimeoutSeconds = seconds;
                    break;
                default: throw new UsageException($"unknown option {name}");
            }
        }
        options.Validate();
        return options;
    }

    // Lets the caller write an error result even when the rest of the command line is unusable.
    public static string? FindOut(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--out" && i + 1 < args.Length) return FullPath(args[i + 1]);
            if (args[i].StartsWith("--out=", StringComparison.Ordinal)) return FullPath(args[i][6..]);
        }
        return null;
    }

    private void Validate()
    {
        if (App.Length == 0) throw new UsageException("--app is required");
        if (Profile.Length == 0) throw new UsageException("--profile is required");
        if (Out.Length == 0) throw new UsageException("--out is required");
        if (string.IsNullOrWhiteSpace(Call)) throw new UsageException("--call is required");
        if (!File.Exists(Path.Combine(App, "AI-Warp-Engine.dll")))
            throw new UsageException($"--app '{App}' does not contain AI-Warp-Engine.dll");
        foreach (var script in Scripts)
            if (!File.Exists(script)) throw new UsageException($"--script '{script}' does not exist");
        if (Media is not null && !Directory.Exists(Media) && !File.Exists(Media))
            throw new UsageException($"--media '{Media}' does not exist");
    }

    private static string FullPath(string path) => Path.GetFullPath(path.Trim().Trim('"'));

    private static Size ParseSize(string value)
    {
        var parts = value.Split('x', 'X');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var width) ||
            !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var height) ||
            width < 200 || height < 100)
            throw new UsageException($"--size needs WIDTHxHEIGHT (for example 1338x804), got '{value}'");
        return new Size(width, height);
    }
}
