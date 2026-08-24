using Helper.FileSystem;

namespace Core.FileSystem;

public class AutoSorter
{
    private readonly FileSystemManager _fileSystem;

    public AutoSorter(string path)
    {
        _fileSystem = new FileSystemManager(path);

        SetCallbacks();
    }

    private void SetCallbacks()
    {
        _fileSystem.FileChanged += OnFileChanged;
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
                OnFileRenamed(change.Path);
                break;
        }
    }

    private void OnFileCreated(string path)
    {
        // Algorithm
    }

    private void OnFileModified(string path)
    {
        // Algorithm
    }

    private void OnFileDeleted(string path)
    {
        // Algorithm
    }

    private void OnFileRenamed(string path)
    {
        // Algorithm
    }

    public void Start()
    {
        _fileSystem.Start();
    }

    public void Stop()
    {
        _fileSystem.Stop();
    }
}