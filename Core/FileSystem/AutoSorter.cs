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
        _fileSystem = new FileSystemManager(path);
        _clipboardWatcher = new ClipboardWatcher();

        SetCallbacks();
    }

    private void SetCallbacks()
    {
        _fileSystem.FileChanged += OnFileChanged;
        _clipboardWatcher.ClipboardChanged += OnClipboardChanged;
    }

    private void OnClipboardChanged(ClipboardChange change)
    {
        Log.AppendLog(
            $"Clipboard {change.Operation}: " +
            string.Join(", ", change.Paths));
    }

    private void OnFileChanged(FileChange change)
    {
        switch (change.Type)
        {
            case FileChangeType.Created:
                OnFileCreated(change.Path);
                break;

            case FileChangeType.Modified:
                OnFileModified(change.Path);
                break;

            case FileChangeType.Deleted:
                OnFileDeleted(change.Path);
                break;

            case FileChangeType.Renamed:
            {
                if (change.OldPath == null)
                {
                    Log.AppendLog(
                        $"Can't get old path, current path is {change.Path}");
                    return;
                }

                OnFileRenamed(
                    change.OldPath,
                    change.Path);

                break;
            }
        }
    }

    private void OnFileCreated(string path)
    {
        // Data Gather
        // Algorithm
    }

    private void OnFileModified(string path)
    {
        // Data Gather
        // Algorithm
    }

    private void OnFileDeleted(string path)
    {
        // Data Gather
        // Algorithm
    }

    private void OnFileRenamed(string oldPath, string newPath)
    {
        _userActionGather.appendRename(
            oldPath,
            newPath);

        // Algorithm
    }

    public void Start()
    {
        _fileSystem.Start();
        _clipboardWatcher.Start();
    }

    public void Stop()
    {
        _fileSystem.Stop();
        _clipboardWatcher.Stop();
    }
}