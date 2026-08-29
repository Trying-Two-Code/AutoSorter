using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json.Nodes;
using Newtonsoft;
using System.Windows;
using System.Diagnostics;

namespace App.Ui.UiScripts;

class UserSettings
{
    public static readonly string UserFileDataPath = "Ui/UserSettings/userSettings.json";

    public UserSettings()
    {
        
    }

    public static bool running = false;
    public static int promptUserAmm = 50;
    public static bool muted = false;
    public static void GetSettings()
    {
        running =       JsonFileReader.Read<bool>("running");
        promptUserAmm = JsonFileReader.Read<int>("promptUserAmm");
        muted =         JsonFileReader.Read<bool>("muted");
    }


    public static T Set<T>(string key, T value)
    {
        JsonFileWriter.FilePath = UserFileDataPath;
        return JsonFileWriter.Write<T>(key, value);
    }

    public static class JsonFileReader
    {
        public static T Read<T>(string filePath, string key)
        {
            string json = File.ReadAllText(filePath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return (T)jsonObj[key];
        }
        public static T Read<T>(string key)
        {
            string json = File.ReadAllText(UserFileDataPath);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return (T)jsonObj[key];
        }
    }

    public static class JsonFileWriter
    {
        public static string? FilePath;
        public static T Write<T>(string key, T value)
        {
            if (FilePath == null) return default;

            string json = File.ReadAllText(FilePath);
            if (json == null) return default;

            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            if (jsonObj == null) return default;

            jsonObj[key] = value;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(FilePath, output);
            return value;
        }
    }
}