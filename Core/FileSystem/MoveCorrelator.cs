using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Core.FileSystem;

/// <summary>
/// A single source file/folder that was Cut and is now waiting to be matched
/// with the filesystem events of the actual move.
/// </summary>
public class PendingMove
{
    public string SourcePath { get; set; } = "";

    /// <summary>-1 when the size is unknown (for example a folder).</summary>
    public long SourceSize { get; set; } = -1;

    public DateTime CutAtUtc { get; set; }

    /// <summary>
    /// Set once the Deleted event for the source path has been observed.
    /// </summary>
    public DateTime? DeletedAtUtc { get; set; }

    /// <summary>Last time this pending move transitioned to a new state.</summary>
    public DateTime UpdatedAtUtc { get; set; }
}

/// <summary>The resolved old/new paths of a correlated move.</summary>
public class MoveCorrelationResult
{
    public string OldPath { get; set; } = "";
    public string NewPath { get; set; } = "";

    public MoveCorrelationResult(string oldPath, string newPath)
    {
        OldPath = oldPath;
        NewPath = newPath;
    }
}

/// <summary>
/// Correlates clipboard Cut operations / filesystem Delete events with 
/// filesystem create events so the destination of a move can be found.
///
/// Only sources located inside the configured source root (the monitored
/// folder, e.g. Downloads) are ever tracked. The destination of a tracked
/// move may be anywhere on the filesystem. Filesystem activity that cannot be
/// tied to a pending source is simply ignored, which keeps the system quiet
/// when the user is not moving a file out of the monitored folder.
///
/// Sizes are supplied by the caller and the clock is injectable, which makes
/// this component deterministic and unit-testable.
/// </summary>
public class MoveCorrelator
{
    /// <summary>
    /// Correlation window. A stale pending source can never match an
    /// unrelated future creation once this window has passed.
    /// </summary>
    public static readonly TimeSpan DefaultTimeout =
        TimeSpan.FromSeconds(30);

    private readonly object _lock = new();
    private readonly string _sourceRoot;
    private readonly TimeSpan _timeout;
    private readonly Func<DateTime> _clock;

    private readonly List<PendingMove> _pending = new();

    public MoveCorrelator(
        string sourceRoot,
        TimeSpan? timeout = null,
        Func<DateTime>? clock = null)
    {
        _sourceRoot = NormalizePath(sourceRoot);
        _timeout = timeout ?? DefaultTimeout;
        _clock = clock ?? (() => DateTime.UtcNow);
    }

    /// <summary>Number of currently pending (non-expired) moves.</summary>
    public int PendingCount()
    {
        lock (_lock)
        {
            CleanupExpired();
            return _pending.Count;
        }
    }

    /// <summary>
    /// True when there is at least one pending move. Used as a gate so
    /// unrelated filesystem events are ignored while nothing is pending.
    /// </summary>
    public bool HasActivePending()
    {
        lock (_lock)
        {
            CleanupExpired();
            return _pending.Count > 0;
        }
    }

    /// <summary>
    /// Records one Cut source. Sizes are supplied by the caller.
    /// Returns true when the source was accepted (it is inside the source
    /// root and not already pending); false when it should be ignored.
    /// </summary>
    public bool RecordCutSource(string sourcePath, long sourceSize)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            return false;

        lock (_lock)
        {
            CleanupExpired();

            string normalized = NormalizePath(sourcePath);

            if (!IsInsideSourceRoot(normalized))
                return false;

            if (_pending.Any(p => p.SourcePath.Equals(
                    normalized,
                    StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            DateTime now = _clock();

            _pending.Add(new PendingMove
            {
                SourcePath = normalized,
                SourceSize = sourceSize,
                CutAtUtc = now,
                UpdatedAtUtc = now
            });

            return true;
        }
    }

    /// <summary>
    /// Correlates a Deleted event with a pending Cut source. The path must be
    /// exactly the pending source path (case-insensitive); it never matches by
    /// name alone.
    /// </summary>
    public bool NotifyDeleted(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        lock (_lock)
        {
            CleanupExpired();

            string normalized = NormalizePath(path);

            PendingMove? match = _pending.FirstOrDefault(p =>
                p.DeletedAtUtc == null &&
                p.SourcePath.Equals(normalized, StringComparison.OrdinalIgnoreCase));

            if (match == null && !UserDeleted(path))
                return false;

            DateTime now = _clock();

            bool MakeMatch()
            {
                match = new PendingMove
                {
                    SourcePath = normalized,
                    DeletedAtUtc = now,
                    UpdatedAtUtc = now
                };
                _pending.Add(match);
                return true;
            }
            if (match == null)
                return MakeMatch();

            match.DeletedAtUtc = now;
            match.UpdatedAtUtc = now;

            return true;
        }
    }

    /// <summary>
    /// Filter used to detect if a user drag moved a file. Returns false
    /// if the path belongs to a background process.
    /// </summary>
    public bool UserDeleted(string path)
    {
        //TODO: check if deletion is background process or user action
        return true;
    }

    /// <summary>
    /// Cheap pre-filter used before a size lookup is performed. Returns true
    /// when the created path could still be the destination of a pending move
    /// (the file name matches a source that was already deleted).
    /// </summary>
    public bool IsCandidateCreated(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        lock (_lock)
        {
            CleanupExpired();

            string createdName = Path.GetFileName(path);

            return _pending.Any(p =>
                p.DeletedAtUtc != null &&
                Path.GetFileName(p.SourcePath).Equals(createdName, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Correlates a Created event (anywhere on the filesystem) with a pending
    /// move that is awaiting its destination. A move is only inferred when the
    /// source was actually deleted first and the created path matches by name
    /// and — when both sizes are known — does not conflict on size.
    /// </summary>
    public MoveCorrelationResult? NotifyCreated(string path, long size)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        lock (_lock)
        {
            CleanupExpired();

            string createdName = Path.GetFileName(path);

            PendingMove? match = _pending
                .Where(p => p.DeletedAtUtc != null)
                .Where(p => Path.GetFileName(p.SourcePath)
                    .Equals(createdName, StringComparison.OrdinalIgnoreCase))
                .Where(p => !SizesConflict(p.SourceSize, size))
                .OrderBy(p => p.DeletedAtUtc)
                .FirstOrDefault();

            if (match == null)
                return null;

            _pending.Remove(match);

            return new MoveCorrelationResult(match.SourcePath, path);
        }
    }

    /// <summary>
    /// Correlates a Renamed event. A same-volume cut-and-paste is reported by
    /// the watcher as a rename, so the event's old path directly identifies the
    /// pending source (full-path identity, never name-only).
    /// </summary>
    public MoveCorrelationResult? NotifyRenamed(
        string oldPath,
        string newPath,
        long newSize)
    {
        if (string.IsNullOrWhiteSpace(oldPath) ||
            string.IsNullOrWhiteSpace(newPath))
        {
            return null;
        }

        lock (_lock)
        {
            CleanupExpired();

            string normalizedOld = NormalizePath(oldPath);

            PendingMove? match = _pending.FirstOrDefault(p =>
                p.SourcePath.Equals(normalizedOld, StringComparison.OrdinalIgnoreCase) &&
                !SizesConflict(p.SourceSize, newSize));

            if (match == null)
                return null;

            _pending.Remove(match);

            return new MoveCorrelationResult(oldPath, newPath);
        }
    }

    private void CleanupExpired()
    {
        DateTime now = _clock();

        _pending.RemoveAll(p => now - p.UpdatedAtUtc > _timeout);
    }

    private bool IsInsideSourceRoot(string path)
    {
        string root = _sourceRoot;

        if (string.IsNullOrEmpty(root))
            return false;

        if (path.Equals(root, StringComparison.OrdinalIgnoreCase))
            return true;

        // Prevent "C:\Downloads2" from being treated as inside "C:\Downloads".
        return path.Length > root.Length &&
               path.StartsWith(root, StringComparison.OrdinalIgnoreCase) &&
               (path[root.Length] == Path.DirectorySeparatorChar ||
                path[root.Length] == Path.AltDirectorySeparatorChar);
    }

    private static bool SizesConflict(long sourceSize, long newSize)
    {
        // Unknown sizes (-1) never conflict; size is preferred evidence only.
        return sourceSize >= 0 && newSize >= 0 && sourceSize != newSize;
    }

    private static string NormalizePath(string path)
    {
        string normalized = path.Trim();

        // Keep the root separator of drive roots (e.g. "C:\").
        if (normalized.Length > 3)
        {
            normalized = normalized.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
        }

        return normalized;
    }
}
