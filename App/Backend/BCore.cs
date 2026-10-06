namespace BCore;

using Core;

public class AppAPI
{
    private readonly CoreAlgorithm _core;

    public AppAPI(string path, string sourceRoot, UITask _uIEvent)
    {
        _core = new CoreAlgorithm(path, sourceRoot, _uIEvent);
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