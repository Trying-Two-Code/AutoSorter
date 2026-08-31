using Helper.DataGathering;
using Helper.FileSystem;
using System.Diagnostics;

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
    public static string[] KnownBackgroundFolders = [];
    private static bool IsProbablyUserAction(string file)
    {
        bool output = true;
        //check if file belongs to AutoSorter
        if (file.Contains(@"AutoSorter\bin\App") || file.Contains(@"AutoSorter\.git\refs"))
            return false;

        //check if many actions are happening at once (too fast and it's probably not the user)


        //check if the file belongs to any folders that are used in background
        if (KnownBackgroundFolders.Any(KnownBackgroundFolder =>
        {
            return KnownBackgroundFolder.Contains(file);
        }))
        {
            output = false;
        }

        //check if the filetype is one of the types that gamers usually edit
        string[] UserFileType = ["image", "text", "audio", "video", "font", "application/zip"];
        string extension = Path.GetExtension(file);
        string mimeType = MimeTypes.GetMimeType(extension);
        if (!UserFileType.Any(_fileType => 
        { return mimeType.Contains(_fileType); }
        ))
        {
            output = false;
        }
        

        //check if file belongs to apps
        if (file.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)))
            output = false;
        //Debug.WriteLine("file belongs to app:" + Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

        //check if file has blacklisted (non human) extension
        string[] BlacklistedExtensions = [".log"];
        if (BlacklistedExtensions.Any(_blackExtension =>
        { return extension.Contains(_blackExtension); }
        ))
        {
            output = false;
        }



        if (output)
        {
            Log.AppendLog("Check Mime: " + mimeType + " ::: TYPE - Path ::: " + file);
        }

        return output;
    }

    public static string? LastDeletedFile;
    public static string? FindOldPath(string e)
    {
        //TODO: add timeout; add size checking;
        string? e1 = Path.GetFileName(e);
        string? e2 = Path.GetFileName(LastDeletedFile);

        bool CheckEqual(string? e1, string? e2)
        {
            if (e1 != null && e2 != null)
                return (e1 == e2);
            else
                return false;
        }

        return CheckEqual(e1, e2) ? LastDeletedFile : null;
    }

    private void OnFileChanged(FileChange change)
    {
        if (!IsProbablyUserAction(change.Path))
            return;

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
        string? oldpath = FindOldPath(path);

        Log.AppendLog(
            $"[FileCreated] Path={path}; OldPath={oldpath}");

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
        LastDeletedFile = path;

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