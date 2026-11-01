using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace NativeHost;

internal static class NativeMethods
{
    private const uint WsOverlappedWindow = 0x00CF0000;
    private const uint GwEnabledPopup = 6;
    private const uint MonitorDefaultToNearest = 2;
    private const int MdtEffectiveDpi = 0;
    private const int DialogTextId = 0xFFFF;
    private const uint WmClose = 0x0010;

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect { public int Left, Top, Right, Bottom; }

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromPoint(Point point, uint flags);

    [DllImport("shcore.dll")]
    private static extern int GetDpiForMonitor(IntPtr monitor, int type, out uint dpiX, out uint dpiY);

    [DllImport("user32.dll")]
    private static extern bool AdjustWindowRectExForDpi(ref Rect rect, uint style, bool menu, uint exStyle, uint dpi);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr window, uint command);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDlgItem(IntPtr dialog, int id);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr window, StringBuilder text, int capacity);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr window, StringBuilder name, int capacity);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr window, out Rect rect);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr window, out Rect rect);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ProcessEntry
    {
        public uint Size;
        public uint Usage;
        public uint ProcessId;
        public UIntPtr DefaultHeapId;
        public uint ModuleId;
        public uint Threads;
        public uint ParentProcessId;
        public int PriorityClassBase;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string ExeFile;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateToolhelp32Snapshot(uint flags, uint processId);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "Process32FirstW")]
    private static extern bool Process32First(IntPtr snapshot, ref ProcessEntry entry);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, EntryPoint = "Process32NextW")]
    private static extern bool Process32Next(IntPtr snapshot, ref ProcessEntry entry);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr handle);

    // WinForms caches sizes, so it can disagree with the window Windows actually created.
    public static Size ClientRectSize(IntPtr window) =>
        GetClientRect(window, out var rect) ? new Size(rect.Right - rect.Left, rect.Bottom - rect.Top) : Size.Empty;

    public static Rectangle WindowBounds(IntPtr window) =>
        GetWindowRect(window, out var rect) ? Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom) : Rectangle.Empty;

    public static uint MonitorDpi(Screen screen)
    {
        var center = new Point(screen.Bounds.Left + screen.Bounds.Width / 2, screen.Bounds.Top + screen.Bounds.Height / 2);
        var monitor = MonitorFromPoint(center, MonitorDefaultToNearest);
        return monitor != IntPtr.Zero && GetDpiForMonitor(monitor, MdtEffectiveDpi, out var dpi, out _) == 0 ? dpi : 96;
    }

    // Non-client size (frame plus caption) of a regular top-level window at the given DPI.
    public static Size FrameSize(uint dpi)
    {
        var rect = new Rect();
        return AdjustWindowRectExForDpi(ref rect, WsOverlappedWindow, false, 0, dpi)
            ? new Size(rect.Right - rect.Left, rect.Bottom - rect.Top)
            : new Size(16, 39);
    }

    // The app reports fatal startup errors with a modal MessageBox owned by its form.
    // GetWindow returns the owner itself when there is no enabled popup.
    public static IntPtr FindModalDialog(IntPtr owner)
    {
        var popup = GetWindow(owner, GwEnabledPopup);
        return popup != IntPtr.Zero && popup != owner && ClassName(popup) == "#32770" ? popup : IntPtr.Zero;
    }

    public static string DescribeDialog(IntPtr dialog)
    {
        var title = Text(dialog);
        var body = Text(GetDlgItem(dialog, DialogTextId));
        return string.IsNullOrEmpty(title) ? body : $"{title}: {body}";
    }

    public static void CloseWindow(IntPtr window) => PostMessage(window, WmClose, IntPtr.Zero, IntPtr.Zero);

    public static void KillTree(Process? process)
    {
        try
        {
            if (process is { HasExited: false }) process.Kill(entireProcessTree: true);
        }
        catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
        }
    }

    // WebView2 browser processes are direct children of the process that hosts the control. They are found by parent id,
    // which also covers a browser that never finished initializing (its id is then unknown to the control).
    public static List<Process> OwnedBrowserProcesses()
    {
        var owned = new List<Process>();
        var self = Process.GetCurrentProcess();
        var ids = new List<uint>();
        var snapshot = CreateToolhelp32Snapshot(0x2, 0);
        if (snapshot == new IntPtr(-1)) return owned;
        try
        {
            var entry = new ProcessEntry { Size = (uint)Marshal.SizeOf<ProcessEntry>() };
            for (var more = Process32First(snapshot, ref entry); more; more = Process32Next(snapshot, ref entry))
                if (entry.ParentProcessId == (uint)self.Id && string.Equals(entry.ExeFile, "msedgewebview2.exe", StringComparison.OrdinalIgnoreCase))
                    ids.Add(entry.ProcessId);
        }
        finally
        {
            CloseHandle(snapshot);
        }
        foreach (var id in ids)
        {
            try
            {
                var process = Process.GetProcessById((int)id);
                // A recycled parent id could otherwise make an unrelated older process look like our child.
                if (process.StartTime >= self.StartTime) owned.Add(process);
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException or System.ComponentModel.Win32Exception)
            {
            }
        }
        return owned;
    }

    public static void KillOwnedBrowsers()
    {
        foreach (var process in OwnedBrowserProcesses()) KillTree(process);
    }

    private static string Text(IntPtr window)
    {
        if (window == IntPtr.Zero) return "";
        var text = new StringBuilder(1024);
        GetWindowText(window, text, text.Capacity);
        return text.ToString().Trim();
    }

    private static string ClassName(IntPtr window)
    {
        var name = new StringBuilder(256);
        GetClassName(window, name, name.Capacity);
        return name.ToString();
    }
}
