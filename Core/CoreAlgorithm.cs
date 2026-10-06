using Core.FileSystem;
using Core.Sorter;
using System;
using System.Collections.Generic;
using System.Text;


namespace Core;

public delegate Task<bool> UITask(object sender, EventArgs args);

public class CoreAlgorithm
{
    // Core entry point.
    // Holds and coordinates all core algorithms/subsystems.
    // AutoSorter is only one algorithm; future algorithms can be added here
    // without putting their implementation directly into this class.

    private readonly AutoSorter _autoSorter;
    private readonly SortAlgorithm _sortAlgorithm;
    private readonly PromptUser _promptUser;


    public CoreAlgorithm(String path, String sourceRoot, UITask _uIEvent)
    {
        _autoSorter = new AutoSorter(path, sourceRoot);
        _promptUser = new PromptUser(_uIEvent);
        _sortAlgorithm = new SortAlgorithm(_promptUser);

        _autoSorter.SendAllDataInstance.OnFileMoveEvent += _sortAlgorithm.OnDataGained;
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