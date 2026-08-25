namespace Helper.API.Win32;

using System.Runtime.InteropServices;

public static class Shell32
{
    [DllImport(
        "shell32.dll",
        CharSet = CharSet.Unicode)]
    public static extern uint DragQueryFile(
        IntPtr hDrop,
        uint iFile,
        char[]? lpszFile,
        uint cch);
}