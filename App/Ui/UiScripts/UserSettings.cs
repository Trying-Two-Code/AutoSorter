using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Path = System.IO.Path;

namespace App.Ui.UiScripts;

static class UserSettings
{
    public static readonly string UserFileDataPath =
        Path.Combine(AppContext.BaseDirectory, @"Ui\UserSettings\userSettings.json");

    public static JSONSettingsMinipulator JSONMinipulator = new("UserSettings.json", "UserSettings");

    public static Settings GetSettings()
    {
        Settings settings = JSONMinipulator.Data;

        return settings;
    }
    
    public static object? GetSettings(string key)
    {
        Settings settings = GetSettings();

        object? value = typeof(Settings)?.GetProperty(key)?.GetValue(settings);

        return value;
    }

    public static void Set(Settings settings)
    {
        JSONMinipulator.Data = settings;
    }

    public static void Set(string key, object? value)
    {
        Settings settings = GetSettings();
        typeof(Settings)?.GetProperty(key)?.SetValue(settings, value);
        JSONMinipulator.Data = settings;
        Debug.Assert(value == GetSettings(key));
    }

}
public class JSONSettingsMinipulator
{
    private static Mutex Mut;
    public string MyPath { get; set; }
    public bool Active { get; set; }
    public Settings Data
    {
        get => Get<Settings>(); 
        set => Set<Settings>(value);
    }

    public JSONSettingsMinipulator(string _path, string _MutexIdentity, Settings? _Data = null)
    {
        MyPath = Path.Combine(AppContext.BaseDirectory, _path);
        Active = true;

        Mut = new(false, $"Global\\{_MutexIdentity}");

        EnsureFileExists();

        _Data ??= Get();
        Data = _Data;
    }

    private void EnsureFileExists()
    {
        bool ExistsAndPopulated = File.Exists(MyPath) && new FileInfo(MyPath).Length != 0;

        if (!ExistsAndPopulated)
        {
            if (Mut.WaitOne(10000))
            {
                CreateDefaultFile(MyPath);
            }
            Mut.ReleaseMutex();
        }
    }

    public T Get<T>()
    {
        T data = default;

        if (Mut.WaitOne(1000))
        {
            using (StreamReader r = new StreamReader(MyPath))
            {
                string d = r.ReadToEnd();
                data = JsonSerializer.Deserialize<T>(d);
            }
        }

        Mut.ReleaseMutex();

        return data;
    }

    public Settings Get()
    {
        return Get<Settings>();
    }

    public void Set<T>(T data)
    {
        if (Mut.WaitOne(1000))
        {
            using (StreamWriter r = new StreamWriter(MyPath))
            {
                string json = JsonSerializer.Serialize(data);
                r.Write(json);
            }
        }
        Mut.ReleaseMutex();
    }

    public void Set(Settings data)
    {
        Set<Settings>(data);
    }

    private static void CreateDefaultFile(string path)
    {
        string? pathDirectory = Path.GetDirectoryName(path);
        if (pathDirectory == null) return;

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

public class Settings()
{
    public bool running { get; set; } = true;
    public int promptUserAmm { get; set; } = 50;
    public bool muted { get; set; } = false;
}