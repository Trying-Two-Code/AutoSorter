using Helper.FileSystem;
using Helper.DataGathering;

namespace Core.FileSystem;

public class AutoSorter
{
    private readonly FileSystemManager _fileSystem;
    private readonly ClipboardWatcher _clipboardWatcher;

    private readonly UserActionGather _userActionGather = new();

    public AutoSorter(string path)
    {
        Log.AppendLog($"AutoSorter initializing: {path}");

        _fileSystem = new FileSystemManager(path);
        _clipboardWatcher = new ClipboardWatcher();

        SetCallbacks();

        Log.AppendLog("AutoSorter initialized.");
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
        Log.AppendLog(
            $"[ClipboardChanged] " +
            $"Operation={change.Operation}, " +
            $"Paths=[{string.Join(", ", change.Paths)}]");

        // Data Gather
        // Algorithm
    }

    private void OnFileChanged(FileChange change)
    {
        switch (change.Type)
        {
            case FileChangeType.Created:
                OnFileCreated(change.Path);
                break;

            case FileChangeType.Deleted:
                OnFileDeleted(change.Path);
                break;

            case FileChangeType.Renamed:
            {
                if (change.OldPath == null)
                {
                    Log.AppendLog(
                        $"[FileRenamed] Missing old path: {change.Path}");

                    return;
                }

                OnFileRenamed(
                    change.OldPath,
                    change.Path);

                break;
            }

            case FileChangeType.Modified:

                // FileSystemWatcher can emit multiple
                // Changed events during one operation.
                // Ignore directory modifications for now.

                if (Directory.Exists(change.Path))
                    return;

                OnFileModified(change.Path);
                break;
        }
    }

    private void OnFileCreated(string path)
    {
        Log.AppendLog(
            $"[FileCreated] Path={path}");

        // Data Gather
        // Algorithm
    }

    private void OnFileModified(string path)
    {
        Log.AppendLog(
            $"[FileModified] Path={path}");

        // Data Gather
        // Algorithm
    }

    private void OnFileDeleted(string path)
    {
        Log.AppendLog(
            $"[FileDeleted] Path={path}");

        // Data Gather
        // Algorithm

        _userActionGather.appendDelete(path);
    }

    private void OnFileCopy(
        string oldPath,
        string newPath)
    {
        Log.AppendLog(
            $"[FileCopy] " +
            $"From={oldPath}, " +
            $"To={newPath}");

        _userActionGather.appendCopy(
            oldPath,
            newPath);

        // Algorithm
    }

    private void OnFileRenamed(
        string oldPath,
        string newPath)
    {
        Log.AppendLog(
            $"[FileRenamed] " +
            $"From={oldPath}, " +
            $"To={newPath}");

        _userActionGather.appendRename(
            oldPath,
            newPath);

        // Algorithm
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
}