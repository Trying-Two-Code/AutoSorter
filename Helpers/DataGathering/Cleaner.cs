using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Helpers.DataGathering
{
    public static class Cleaner
    {
        public static void LogCreated(string content)
        {
            //maybe not needed...
        }

        public static Mutex mut = new Mutex(false, "Global\\Cleaner");
        public static Dictionary<string, bool> PreviousCalculations { get; set; } = new();

        public static bool IsClean(string content)
        {
            mut.WaitOne();
            bool result = PreviousCalculations.ContainsKey(content);
            if (result == true)
            {
                mut.ReleaseMutex();
                return false;
                return PreviousCalculations[content];
            }

            result = UserFile(content);
            PreviousCalculations.Add(content, result);

            mut.ReleaseMutex();

            return (bool)result;
        }

        /// <summary>
        /// Filter used to detect if a file conforms to general User expectations
        /// </summary>
        public static bool UserFile(string path)
        {
            //TODO: check if deletion is background process or user action

            //TODO: Is in background process path:
            bool IsBackgroundPath()
            {
                string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string AppDataPath2 = Environment.ExpandEnvironmentVariables("%AppData%");
                string LocalAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string AutosorterPath = Environment.ProcessPath;
                string[] BackgroundPaths = [
                    AppDataPath, LocalAppDataPath,
                AppDataPath2, AutosorterPath]; //TODO: add more

                if (BackgroundPaths.Any(
                    _BackgroundPath =>
                    {
                        return path.Contains(_BackgroundPath);
                    }))
                    return true;
                else
                    return false;
            }

            //TODO: Is a background extension:
            bool IsBackgroundExtension()
            {
                string[] BackgroundExtensions = [
                    ".tmp", ".lock", ".exe", ".dll", ".baml",
                ".cs", ".cache", ".pbd", ".pdb", ".sqlite-journal",
            ];
                if (BackgroundExtensions.Any(
                    _BackgroundExtension =>
                    {
                        return Path.GetExtension(path) == _BackgroundExtension;
                    }))
                    return true;
                else
                    return false;
            }

            bool[] DependantMethods = [
                IsBackgroundPath(),
            IsBackgroundExtension()];

            return !DependantMethods.Contains(true); //only return true if everything is false
        }
    }
}
