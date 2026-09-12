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
        public static void RunOnStartup()
        {
            //Send to registry
            string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(RegistryPath, true);

            string ExecutionPath = Application.ExecutablePath.ToString();

            if (regKey.GetValue("AutoSorter") == ExecutionPath)
            {
                //Already registered.
            }
            else
            {
                regKey.SetValue("AutoSorter", $"{ExecutionPath} --open-closed \"1\"");

                //In future, users may manually call if needed:
                //CreateShortcut(StartupFolder, ExecutionPath);
            }
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
