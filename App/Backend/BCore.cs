namespace BCore;

using Core;

public class AppAPI
{
    private readonly CoreAlgorithm _core;

    public AppAPI(string path, string sourceRoot)
    {
        _core = new CoreAlgorithm(path, sourceRoot);
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