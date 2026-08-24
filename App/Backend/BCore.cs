namespace BCore;

using Core;

public class AppAPI
{
    private readonly CoreAlgorithm _core;

    public AppAPI(string path)
    {
        _core = new CoreAlgorithm(path);
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