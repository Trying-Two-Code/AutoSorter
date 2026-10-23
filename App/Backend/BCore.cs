using Core;

namespace BCore;

public class AppAPI
{
    private readonly CoreAlgorithm _core;

    public AppAPI(string path, string sourceRoot, UITask _uIEvent, GetSettingsTask _userSettings)
    {
        _core = new CoreAlgorithm(path, sourceRoot, _uIEvent, _userSettings);
    }

    public void Start()
    {
        _core.Start();
    }

    public void Stop()
    {
        _core.Stop();
    }
}