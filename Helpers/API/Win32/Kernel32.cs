namespace Helper.API.Win32;

using System;
using System.Runtime.InteropServices;

public static class Kernel32
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr GlobalLock(
        IntPtr hMem);

    [DllImport("kernel32.dll")]
    public static extern bool GlobalUnlock(
        IntPtr hMem);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetModuleHandle(
        string? lpModuleName);
}