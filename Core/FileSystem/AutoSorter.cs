using Helper.DataGathering;
using Helper.FileSystem;

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
        // Keep the system quiet: when nothing has been cut from the monitored
        // folder, unrelated filesystem activity is ignored entirely.
        if (!_moveCorrelator.HasActivePending())
            return;

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