namespace Helper.DataGathering;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;


public enum Action
{
    Move = 0, //crl+ x
    Rename,
    Copy, //crl+ c
    Delete
}

public class UserActionGatherLine
{
    [JsonPropertyName("Action")]
    public Action Action { get; set; }

    [JsonPropertyName("Content")]
    public string Content { get; set; } = "";

    [JsonPropertyName("From")]
    public string? From { get; set; } = null;

    [JsonPropertyName("To")]
    public string? To   { get; set; } = null;

    [JsonPropertyName("Timestamp")]
    public DateTime Timestamp { get; set; }
}

public class UserActionGatherFile
{
    [JsonPropertyName("Lines")]
    public List<UserActionGatherLine> Lines { get; set; } = new();
}

public class UserActionGather
{
    public static Lock _lock = new();
    private static Mutex mut = new Mutex(false, "Global\\UserActionMutex");
    private const string _path = @"./data/userAction.json";

    public void appendMove(string from, string to)
    {
        UserActionGatherFile data = getFileDataFromJSON();
        
        data.Lines.Add(new UserActionGatherLine
        {
            Action = Action.Move,
            Content = $"move from:{from} to:{to}",
            From = from,
            To = to,
            Timestamp = DateTime.UtcNow
        });

        writeFileDataToJSON(data);
    }

    public void appendRename(string from, string to)
    {
        UserActionGatherFile data = getFileDataFromJSON();

        data.Lines.Add(new UserActionGatherLine
        {
            Action = Action.Rename,
            Content = $"rename from:{from} to:{to}",
            From = from,
            To = to,
            Timestamp = DateTime.UtcNow
        });

        writeFileDataToJSON(data);
    }

    public void appendCopy(string from, string to)
    {
        UserActionGatherFile data = getFileDataFromJSON();
        
        data.Lines.Add(new UserActionGatherLine
        {
            Action = Action.Copy,
            Content = $"copy from:{from} to:{to}",
            From = from,
            To = to,
            Timestamp = DateTime.UtcNow
        });

        writeFileDataToJSON(data);
    }

    public void appendDelete(string file)
    {
        UserActionGatherFile data = getFileDataFromJSON();

        UserActionGatherLine userActionGatherLine = new UserActionGatherLine
        {
            Action = Action.Delete,
            Content = $"delete {file}",
            From = file,
            Timestamp = DateTime.UtcNow
        };
        data.Lines.Add(userActionGatherLine);

        writeFileDataToJSON(data);
    }

    public UserActionGatherFile getFileDataFromJSON()
    {
        if (!File.Exists(_path))
            return new UserActionGatherFile();

        UserActionGatherFile? fileData = null;
        try
        {
            mut.WaitOne();

            string json = File.ReadAllText(_path);

            if (string.IsNullOrWhiteSpace(json))
                return new UserActionGatherFile();

            fileData = JsonSerializer.Deserialize<UserActionGatherFile>(json);
        }
        finally
        {
            mut.ReleaseMutex();
        }

        return fileData ?? new UserActionGatherFile();
    }

    private void writeFileDataToJSON(UserActionGatherFile data)
    {
        try
        {
            mut.WaitOne();

            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

            string json = JsonSerializer.Serialize(
                data,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(_path, json);
        }
        finally
        {
            mut.ReleaseMutex();
        }
    }
}