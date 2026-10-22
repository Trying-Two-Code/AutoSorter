using Core.FileSystem;
using Core.Sorter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core;

public delegate Task<bool> UITask(object sender, EventArgs args);
public delegate object GetSettingsTask(object sender, EventArgs args);
public delegate List<object> GetUserActionTask(object sender, EventArgs args);

public class CoreAlgorithm
{
    // Core entry point.
    // Holds and coordinates all core algorithms/subsystems.
    // AutoSorter is only one algorithm; future algorithms can be added here
    // without putting their implementation directly into this class.

    private readonly AutoSorter _autoSorter;
    private readonly SortAlgorithm _sortAlgorithm;
    private readonly PromptUser _promptUser;
    private readonly GetSettingsTask _myGetSettings;

    public CoreAlgorithm(String path, String sourceRoot, UITask _uIEvent, GetSettingsTask _getSettings)
    {
        _autoSorter = new AutoSorter(path, sourceRoot);
        _promptUser = new PromptUser(_uIEvent);

        GetUserActionTask _getUserAction = _autoSorter.SendAllDataInstance.GetUserActions;

        _sortAlgorithm = new SortAlgorithm(sourceRoot, _promptUser, _getSettings, _getUserAction);
        _myGetSettings = _getSettings;

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