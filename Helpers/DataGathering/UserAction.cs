namespace Helper.DataGathering;

using Core.Sorter;
using Helpers.DataGathering;
using JsonRepairSharp;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;

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
    //public static Lock _lock = new();
    //private static Mutex mut = new Mutex(false, "Global\\UserActionMutex"); 
    private JSONDataMinipulator<UserActionGatherLine> _JSONMinipulator = new(@"./data/userAction.json", "UserActionMutex");

    public void appendMove(string from, string to)
    {
        UserActionGatherLine newData = new UserActionGatherLine()
        {
            Action = Action.Move,
            Content = $"move from:{from} to:{to}",
            From = from,
            To = to,
            Timestamp = DateTime.UtcNow
        };

        append(newData);
    }

    public void appendRename(string from, string to)
    {
        UserActionGatherLine newData = new UserActionGatherLine()
        {
            Action = Action.Rename,
            Content = $"rename from:{from} to:{to}",
            From = from,
            To = to,
            Timestamp = DateTime.UtcNow
        };

        append(newData);
    }

    public void appendCopy(string from, string to)
    {
        UserActionGatherLine newData = new UserActionGatherLine()
        {
            Action = Action.Copy,
            Content = $"copy from:{from} to:{to}",
            From = from,
            To = to,
            Timestamp = DateTime.UtcNow
        };

        append(newData);
    }

    public void appendDelete(string file)
    {
        UserActionGatherLine newData = new UserActionGatherLine()
        {
            Action = Action.Delete,
            Content = $"delete {file}",
            From = file,
            Timestamp = DateTime.UtcNow
        };

        append(newData);
    }

    public void append(UserActionGatherLine newData)
    {
        UserActionGatherFile data = GetOldFileData();
        data.Lines.Add(newData);

        _JSONMinipulator.Set(data.Lines);
    }

    private static UserActionGatherFile oldData = new();
    public UserActionGatherFile getFileDataFromJSON()
    {
        List<UserActionGatherLine> data = _JSONMinipulator.Get();
        UserActionGatherFile file = new()
        {
            Lines = data
        };
        return file;
    }

    /// <summary>
    /// Old data gets turned into new data.
    /// Reason this function exists: to be quick
    /// </summary>
    /// <returns>old data</returns>
    public UserActionGatherFile GetOldFileData()
    {
        //no point in returning an old useless data
        if (oldData == null || oldData.Lines.Count == 0)
        {
            oldData = getFileDataFromJSON();
        }
        else
        {
            Task.Run(() =>
            {
                oldData = getFileDataFromJSON();
            });
        }
        return oldData;
    }
}