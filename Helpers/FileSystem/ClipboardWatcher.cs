namespace Helper.FileSystem;

using System.Runtime.InteropServices;
using System.Threading;
using Helper.API.Win32;

public enum ClipboardOperation
{
    Copy,
    Cut
}

public record ClipboardChange(
    ClipboardOperation Operation,
    IReadOnlyList<string> Paths
);

public class ClipboardWatcher : IDisposable
{
    private const uint CF_HDROP = 15;

    private const string PreferredDropEffect =
        "Preferred DropEffect";

    private const uint WM_QUIT = 0x0012;

    private readonly Thread _thread;
    private readonly ManualResetEventSlim _ready = new(false);

    private User32.WndProc? _wndProc;

    private IntPtr _hwnd;

    private bool _running;

    public event Action<ClipboardChange>? ClipboardChanged;

    public ClipboardWatcher()
    {
        _thread = new Thread(MessageLoop)
        {
            IsBackground = true,
            Name = "ClipboardWatcher"
        };

        _thread.Start();

        _ready.Wait();

        if (_hwnd == IntPtr.Zero)
            throw new InvalidOperationException(
                "Failed to create clipboard watcher window.");
    }

    public void Start()
    {
        if (_running)
            return;

        if (!User32.AddClipboardFormatListener(_hwnd))
            throw new InvalidOperationException(
                "Failed to register clipboard listener.");

        _running = true;
    }

    public void Stop()
    {
        if (!_running)
            return;

        User32.RemoveClipboardFormatListener(_hwnd);

        _running = false;
    }

    private void MessageLoop()
    {
        _wndProc = WndProcHandler;

        string className =
            $"AutoSorterClipboardWatcher_{Environment.ProcessId}";

        User32.WNDCLASS wndClass = new()
        {
            lpfnWndProc = _wndProc,
            hInstance = Kernel32.GetModuleHandle(null),
            lpszClassName = className
        };

        User32.RegisterClass(ref wndClass);

        _hwnd = User32.CreateWindowEx(
            0,
            className,
            "AutoSorter Clipboard Watcher",
            0,
            0,
            0,
            0,
            0,
            User32.HWND_MESSAGE,
            IntPtr.Zero,
            wndClass.hInstance,
            IntPtr.Zero);

        _ready.Set();

        if (_hwnd == IntPtr.Zero)
            return;

        while (User32.GetMessage(
                   out User32.MSG msg,
                   IntPtr.Zero,
                   0,
                   0) > 0)
        {
            User32.TranslateMessage(ref msg);
            User32.DispatchMessage(ref msg);
        }
    }

    private IntPtr WndProcHandler(
        IntPtr hwnd,
        uint msg,
        IntPtr wParam,
        IntPtr lParam)
    {
        if (msg == User32.WM_CLIPBOARDUPDATE)
        {
            ReadClipboard();
        }

        return User32.DefWindowProc(
            hwnd,
            msg,
            wParam,
            lParam);
    }

    private void ReadClipboard()
    {
        if (!User32.OpenClipboard(_hwnd))
            return;

        try
        {
            IntPtr hDrop =
                User32.GetClipboardData(CF_HDROP);

            if (hDrop == IntPtr.Zero)
                return;

            List<string> paths = ReadPaths(hDrop);

            if (paths.Count == 0)
                return;

            ClipboardOperation operation =
                ReadDropEffect();

            ClipboardChanged?.Invoke(
                new ClipboardChange(
                    operation,
                    paths));
        }
        finally
        {
            User32.CloseClipboard();
        }
    }

    private static List<string> ReadPaths(IntPtr hDrop)
    {
        uint count =
            Shell32.DragQueryFile(
                hDrop,
                0xFFFFFFFF,
                null,
                0);

        var paths = new List<string>((int)count);

        for (uint i = 0; i < count; i++)
        {
            uint length =
                Shell32.DragQueryFile(
                    hDrop,
                    i,
                    null,
                    0);

            var buffer =
                new char[length + 1];

            Shell32.DragQueryFile(
                hDrop,
                i,
                buffer,
                length + 1);

            paths.Add(
                new string(
                    buffer,
                    0,
                    (int)length));
        }

        return paths;
    }

    private static ClipboardOperation ReadDropEffect()
    {
        uint format =
            User32.RegisterClipboardFormat(
                PreferredDropEffect);

        if (format == 0)
            return ClipboardOperation.Copy;

        IntPtr data =
            User32.GetClipboardData(format);

        if (data == IntPtr.Zero)
            return ClipboardOperation.Copy;

        IntPtr ptr =
            Kernel32.GlobalLock(data);

        if (ptr == IntPtr.Zero)
            return ClipboardOperation.Copy;

        try
        {
            byte effect =
                Marshal.ReadByte(ptr);

            const byte DROPEFFECT_MOVE = 0x02;

            return (effect & DROPEFFECT_MOVE) != 0
                ? ClipboardOperation.Cut
                : ClipboardOperation.Copy;
        }
        finally
        {
            Kernel32.GlobalUnlock(data);
        }
    }

    public void Dispose()
    {
        Stop();

        if (_hwnd != IntPtr.Zero)
        {
            User32.PostMessage(
                _hwnd,
                WM_QUIT,
                IntPtr.Zero,
                IntPtr.Zero);
        }

        if (_thread.IsAlive)
            _thread.Join();

        _ready.Dispose();
    }
}