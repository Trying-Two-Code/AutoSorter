using Helper.DataGathering;
using Helper.FileSystem;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Core.FileSystem.AutoSorter;
using static Core.FileSystem.SendAllData;

namespace Core.FileSystem;

public class AutoSorter
{
    private readonly FileSystemManager _fileSystem;
    private readonly ClipboardWatcher _clipboardWatcher;

    private readonly UserActionGather _userActionGather = new();

    private readonly MoveCorrelator _moveCorrelator;

    public AutoSorter(
        string watchRoot,
        string sourceRoot)
    {
        Log.AppendLog(
            $"AutoSorter initializing: watch={watchRoot}, sourceRoot={sourceRoot}");

        _fileSystem = new FileSystemManager(watchRoot);
        _clipboardWatcher = new ClipboardWatcher();
        _moveCorrelator = new MoveCorrelator(sourceRoot);

        SetCallbacks();
    }

    private void SetCallbacks()
    {
        Log.AppendLog("Registering callbacks...");

        _fileSystem.FileChanged += OnFileChanged;
        _clipboardWatcher.ClipboardChanged += OnClipboardChanged;

        Log.AppendLog("Callbacks registered.");
    }

    private void OnClipboardChanged(ClipboardChange change)
    {
        // A clipboard Copy must never be treated as a move.
        if (change.Operation != ClipboardOperation.Cut)
            return;

        int queued = 0;

        foreach (string path in change.Paths)
        {
            // Only sources inside the configured root (e.g. Downloads) are
            // tracked; cuts of files anywhere else are ignored quietly.
            if (_moveCorrelator.RecordCutSource(path, TryGetSize(path)))
                queued++;
        }

        if (queued > 0)
            Log.AppendLog($"[PendingMove] Queued {queued} cut source(s).");
    }

    private void OnFileChanged(FileChange change)
    {
        // Problem: totally bypasses drag and drop
        // Keep the system quiet: when nothing has been cut from the monitored
        // folder, unrelated filesystem activity is ignored entirely.
        //if (!_moveCorrelator.HasActivePending())
        //    return;

        switch (change.Type)
        {
            case FileChangeType.Deleted:
            {
                if (_moveCorrelator.NotifyDeleted(change.Path))
                    Log.AppendLog($"[PendingMove] Source deleted: {change.Path}");

                break;
            }

            case FileChangeType.Created:
            {
                // Cheap check before a size lookup: unrelated creations must
                // still not produce any logging or extra work.
                if (!_moveCorrelator.IsCandidateCreated(change.Path))
                    break;

                MoveCorrelationResult? correlated =
                    _moveCorrelator.NotifyCreated(
                        change.Path,
                        TryGetSize(change.Path));

                if (correlated != null)
                    OnFileMove(correlated.NewPath, correlated.OldPath);

                break;
            }

            case FileChangeType.Renamed:
            {
                if (change.OldPath == null)
                    break;

                // Same-volume cut-and-paste shows up as a rename; the old path
                // of the event directly identifies the pending source.
                MoveCorrelationResult? correlated =
                    _moveCorrelator.NotifyRenamed(
                        change.OldPath,
                        change.Path,
                        TryGetSize(change.Path));

                if (correlated != null)
                    OnFileMove(correlated.NewPath, correlated.OldPath);

                break;
            }

            case FileChangeType.Modified:
                // Not used for move correlation.
                break;
        }
    }

    private static long TryGetSize(string path)
    {
        try
        {
            if (File.Exists(path))
                return new FileInfo(path).Length;
        }
        catch
        {
            // Size unavailable (for example a folder or a locked file).
            // The correlator treats unknown sizes as non-conflicting.
        }

        return -1;
    }

    public void OnFileMove(string newPath, string oldPath)
    {
        Log.AppendLog(
            "[FileMoved] " +
            $"From={oldPath}, " +
            $"To={newPath}");

        _userActionGather.appendMove(
            oldPath,
            newPath);

        //TODO: pass all new data or send/get all data another way
        SendAllDataInstance.SendData(newPath, oldPath);
    }

    

    public void Start()
    {
        Log.AppendLog("AutoSorter starting...");

        _fileSystem.Start();
        _clipboardWatcher.Start();

        Log.AppendLog("AutoSorter started.");
    }

    public void Stop()
    {
        Log.AppendLog("AutoSorter stopping...");

        _fileSystem.Stop();
        _clipboardWatcher.Stop();

        Log.AppendLog("AutoSorter stopped.");
    }

    public SendAllData SendAllDataInstance = new();
}

public class OnFileMoveEventArgs : EventArgs
{
    public FileInfo? NewFile { get; set; }
    public FileMoveData[]? AllData { get; set; }
}

/// <summary>
/// A class that allows the autosorter to pass along relavent data
/// whenever user data is received.
/// </summary>
public class SendAllData()
{

    public struct FileMoveData()
    {
        public string OldPath { get; set; } = "";
        public string NewPath { get; set; } = "";
    }

    public event EventHandler<OnFileMoveEventArgs>? OnFileMoveEvent;

    public void SendData(string newPath, string oldPath)
    {
        OnFileMoveEvent?.Invoke(this,
            new OnFileMoveEventArgs
            {
                NewFile = new FileInfo(newPath),
                AllData = AllData(newPath, oldPath)
            });
    }

    public static FileMoveData[] AllData(string newPath, string oldPath)
    {
        List<FileMoveData> allData = [];

        //Add the old stuff
        List<FileMoveData>? oldData = OldData();
        if(oldData != null)
            allData.AddRange(oldData);

        return allData.ToArray();
    }

    static List<JSONDataStructure>? JsonData { get; set; }
    public static List<FileMoveData>? OldData(string? dataPath = null)
    {
        if (JsonData == null)
        {
            RootJSONStructure? rootData = FetchJsonData(dataPath);
            JsonData = rootData?.Lines;
        }

        List <FileMoveData> oldData = new List<FileMoveData>();

        foreach (JSONDataStructure data in JsonData)
        {
            FileMoveData fileMoveData = new()
            {
                NewPath = data.To,
                OldPath = data.From
            };
            oldData.Add(fileMoveData);
        }

        return oldData;
    }

    static readonly string DefaultUserActionPath = Path.Combine(AppContext.BaseDirectory, "data/UserAction.json");
    public static RootJSONStructure? FetchJsonData(string? dataPath)
    {
        dataPath = (dataPath == null) ? DefaultUserActionPath : dataPath;

        if (!File.Exists(dataPath)) return null;

        RootJSONStructure? data;

        using (StreamReader r = new StreamReader(dataPath))
        {
            string json = r.ReadToEnd();
            data = JsonSerializer.Deserialize<RootJSONStructure>(json);
        }

        return data;
    }

    //<THIS MUST MATCH JSON>
    public class RootJSONStructure()
    {
        [JsonPropertyName("Lines")]
        public List<JSONDataStructure> Lines { get; set; } = new();
    }

    public class JSONDataStructure()
    {
        [JsonPropertyName("Action")]
        public int Action {  get; set; }

        [JsonPropertyName("Content")]
        public string Content { get; set; } = "";

        [JsonPropertyName("From")]
        public string From { get; set; } = "";

        [JsonPropertyName("To")]
        public string To { get; set; } = "";

        [JsonPropertyName("Timestamp")]
        public string Timestamp { get; set; } = "";
    }
    //</THIS MUST MATCH JSON>
}