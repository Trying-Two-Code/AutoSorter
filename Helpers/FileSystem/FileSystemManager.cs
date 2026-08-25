namespace Helper.FileSystem;

public enum FileChangeType
{
    Created,
    Modified,
    Deleted,
    Renamed
}

public record FileChange(
    FileChangeType Type,
    string Path
);

public class FileSystemManager
{
    private readonly FileSystemWatcher _watcher;

    public event Action<FileChange>? FileChanged;

    public FileSystemManager(string path)
    {
        _watcher = new FileSystemWatcher(path);

        _watcher.Created += (_, e) =>
            FileChanged?.Invoke(
                new FileChange(FileChangeType.Created, e.FullPath));

        _watcher.Changed += (_, e) =>
            FileChanged?.Invoke(
                new FileChange(FileChangeType.Modified, e.FullPath));

        _watcher.Deleted += (_, e) =>
            FileChanged?.Invoke(
                new FileChange(FileChangeType.Deleted, e.FullPath));

        _watcher.Renamed += (_, e) =>
            FileChanged?.Invoke(
                new FileChange(FileChangeType.Renamed, e.FullPath));

        _watcher.Error += OnError;
    }

    public void Start()
    {
        _watcher.EnableRaisingEvents = true;
    }

    public void Stop()
    {
        _watcher.EnableRaisingEvents = false;
    }

    public void Move(string source, string destination)
    {
        File.Move(source, destination);
    }

    public void OnError(object _, ErrorEventArgs e)
    {
        Console.WriteLine(e.GetException());
    }
}