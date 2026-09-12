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
                if (FileArguments[index + 1] == "1")
                    return true;
            }
            return false;
        }
        public static void RunOnStartup()
        {
            //Send to registry
            string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

            RegistryKey regKey = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
           // Debug.WriteLine("going to regKey: " + regKey);

            string ExecutionPath = System.Windows.Forms.Application.ExecutablePath.ToString();
            //ExecutionPath = @"C:\Users\Drago\source\repos\AutoSorter\bin\App\Debug\net10.0-windows\App.exe";

            if (regKey.GetValue("AutoSorter") != ExecutionPath)
            {
                regKey.SetValue("AutoSorter", System.Windows.Forms.Application.ExecutablePath.ToString() + "--open - closed \"1\"");
                Debug.WriteLine("set regKey value: " + regKey.GetValue("AutoSorter"));
            }

            //as a backup, make a shortcut in startup folder
            string StartupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            CreateShortcut(StartupFolder, ExecutionPath);
        }

        public static void CreateShortcut(string shortCutPath, string shortCutReferences)
        {
            WshShell shell = new WshShell();
            string shortcutAddress = Path.Combine(shortCutPath, @"AutoSorter.lnk");
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "Shortcut to AutoSorter";
            shortcut.Arguments = "--open-closed \"1\"";
            shortcut.TargetPath = shortCutReferences;
            shortcut.WorkingDirectory = Path.GetDirectoryName(shortCutReferences);
            shortcut.Save();
        }
    }
}
