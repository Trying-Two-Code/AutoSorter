using IWshRuntimeLibrary;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;

namespace App.Ui.UiScripts
{
    internal class ActivateOnStartup
    {
        public static bool DetectClose()
        {
            string[] FileArguments = Environment.GetCommandLineArgs();
            if (FileArguments.Contains("--open-closed"))
            {
                int index = Array.IndexOf(FileArguments, "--open-closed");
                bool OpenClosedTrue = index + 1 <= FileArguments.Length && FileArguments[index + 1] == "1";
                return OpenClosedTrue;
            }
            return false;
        }

        public static readonly string? AppDirectory = Environment.ProcessPath;
        public static readonly string RegistryKeyPath = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";
        public static readonly string AppName = "AutoSorter";
        public static readonly string StartClosed = " --open-closed 1 ";
        /// <summary>
        /// Registers app to windows registry, making it start on startup
        /// Note: Only for current user, not all users.
        /// </summary>
        public static void RegisterApp()
        {
            RegistryKey? registryKey = Registry.CurrentUser.OpenSubKey
                (RegistryKeyPath, true);

            if (registryKey == null || AppDirectory == null) return;

            string AppExecutablePath = $"\"{AppDirectory}\"{StartClosed}";
            if ((string?)registryKey.GetValue(AppName) != AppExecutablePath)
                registryKey.SetValue(AppName, AppExecutablePath);
        }

        public static void CreateShortcut(string shortCutPath, string shortCutReferences)
        {
            WshShell shell = new WshShell();
            string shortcutAddress = Path.Combine(shortCutPath, @"AutoSorter.lnk");
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Shortcut to AutoSorter";
            shortcut.Arguments = "--open-closed \"1\""; //makes sure it starts closed.
            shortcut.TargetPath = shortCutReferences;
            shortcut.WorkingDirectory = Path.GetDirectoryName(shortCutReferences);
            shortcut.Save();
        }
    }
}
