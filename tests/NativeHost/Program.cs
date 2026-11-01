namespace NativeHost;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        var report = new Report { OutPath = Options.FindOut(args) };
        var code = 1;
        try
        {
            var options = Options.Parse(args);
            if (options is null)
            {
                Console.WriteLine(Options.Usage);
                return 0;
            }
            report.OutPath = options.Out;
            code = new HostSession(options, report).Run();
        }
        catch (UsageException exception)
        {
            report.Fail("usage: " + exception.Message);
            Console.Error.WriteLine("error: " + exception.Message);
            Console.Error.WriteLine(Options.Usage);
            report.Write();
        }
        catch (Exception exception)
        {
            report.Fail(exception is HarnessException ? exception.Message : exception.ToString());
            report.Write();
        }
        Console.Out.WriteLine(report.Summary());
        Console.Out.Flush();
        Console.Error.Flush();
        // Nothing may keep the process (or its WebView2 children) alive after the verdict.
        NativeMethods.KillOwnedBrowsers();
        Environment.Exit(code);
        return code;
    }
}
