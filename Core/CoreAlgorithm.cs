using Core.FileSystem;

using System;
using System.Collections.Generic;
using System.Text;


namespace Core;

public class CoreAlgorithm
{
    // Core entry point.
    // Holds and coordinates all core algorithms/subsystems.
    // AutoSorter is only one algorithm; future algorithms can be added here
    // without putting their implementation directly into this class.

    private readonly AutoSorter _autoSorter;

    public CoreAlgorithm(String path)
    {
        _autoSorter = new AutoSorter(path);
    }

    public void Start()
    {
        _autoSorter.Start();
    }

    public void Stop()
    {
        _autoSorter.Stop();
    }
}