namespace Helper.DataGathering;

using System.Text.Json;

public enum Action
{
    Move,
    Rename
}

public class UserActionGatherLine
{
    public Action Action { get; set; }
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; }
}

public class UserActionGatherFile
{
    public List<UserActionGatherLine> Lines { get; set; } = new();
}

public class UserActionGather
{
    private const string _path = @"./data/userAction.json";

    public void appendMove(string from, string to)
    {
        UserActionGatherFile data = getFileDataFromJSON();

        data.Lines.Add(new UserActionGatherLine
        {
            Action = Action.Move,
            Content = $"move from:{from} to:{to}",
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
            Timestamp = DateTime.UtcNow
        });

        writeFileDataToJSON(data);
    }


    public UserActionGatherFile getFileDataFromJSON()
    {
        if (!File.Exists(_path))
            return new UserActionGatherFile();

        string json = File.ReadAllText(_path);

        if (string.IsNullOrWhiteSpace(json))
            return new UserActionGatherFile();

        return JsonSerializer.Deserialize<UserActionGatherFile>(json)
               ?? new UserActionGatherFile();
    }

    private void writeFileDataToJSON(UserActionGatherFile data)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

        string json = JsonSerializer.Serialize(
            data,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(_path, json);
    }
}