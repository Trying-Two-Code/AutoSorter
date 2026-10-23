using Newtonsoft;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using static System.Resources.ResXFileRef;
using Path = System.IO.Path;

namespace App.Ui.UiScripts;

static class UserSettings
{
    public static readonly string UserFileDataPath = 
        Path.Combine(AppContext.BaseDirectory, @"Ui\UserSettings\userSettings.json");

    public static object _lock = new();
    public static bool running = false;
    public static int promptUserAmm = 50;
    public static bool muted = false;
    public static void GetSettings()
    {
        lock (_lock)
        {
            running = JsonFileReader.Read<bool>("running");
            promptUserAmm = JsonFileReader.Read<int>("promptUserAmm");
            muted = JsonFileReader.Read<bool>("muted");
        }
    }

    public static T? Set<T>(string key, T value)
    {
        lock (_lock)
        {
            JsonFileWriter.FilePath = UserFileDataPath;
            return JsonFileWriter.Write<T>(key, value);
        }
    }

    public static class JsonFileReader
    {
        public static T? Read<T>(string filePath, string key)
        {
            lock (_lock)
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    dynamic? jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    if (jsonObj != null)
                        return (T?)jsonObj[key];
                }
            }
            return default;
        }

        public static T? Read<T>(string key)
        {
            return Read<T>(UserFileDataPath, key);
        }
    }

    public static class JsonFileWriter
    {
        public static string? FilePath { get; set; } = UserFileDataPath;
        public static T? Write<T>(string key, T value)
        {
            if(FilePath != null)
                if(!MakeSafeToWrite(FilePath))
                    return Write<T>(FilePath, key, value);
            return default;
        }

        public static T? Write<T>(string path, string key, T value)
        {
            lock (_lock)
            {
                if(!MakeSafeToWrite(path)) { return default; }

                dynamic? jsonObj;
                string json = File.ReadAllText(path);

                if (string.IsNullOrEmpty(json))
                {
                    jsonObj = new Newtonsoft.Json.Linq.JObject();
                }
                else
                {
                    jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                }

                if(jsonObj != null)
                    jsonObj[key] = value;

                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(path, output);
                return value;
            }
        }

        private static bool MakeSafeToWrite(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            if (!File.Exists(path)) CreateDefaultFile(path);

            return true;
        }

        private static void CreateDefaultFile(string path)
        {
            string? pathDirectory = Path.GetDirectoryName(path);
            if(pathDirectory == null) return;

            Directory.CreateDirectory(pathDirectory);
            File.WriteAllText(path, 
                "{" +
                    "\r\n  \"running\": true," +
                    "\r\n  \"promptUserAmm\": 15," +
                    "\r\n  \"muted\": false\r\n" +
                "}"
                );
        }
    }
}

public class Settings()
{
    public bool running { get; set; } = true;
    public int promptUserAmm { get; set; } = 50;
    public bool muted { get; set; } = false;

    public static Settings Get()
    {
        UserSettings.GetSettings();

        Settings _settings = new()
        {
            running = UserSettings.running,
            promptUserAmm = UserSettings.promptUserAmm,
            muted = UserSettings.muted
        };

        return _settings;
    }
}