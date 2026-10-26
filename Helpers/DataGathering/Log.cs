using Helpers.DataGathering;
using System.Diagnostics;

namespace Helper.DataGathering;

public static class Log
{
    public static Mutex mut = new(false, "Global\\LoggingMutex");
    private static readonly String _path = "./data/log.txt";

    public static void AppendLog(String content, string? checkClean = null)
    {
        if (checkClean == null || Cleaner.IsClean(checkClean))
        {
            mut.WaitOne(1000);

            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

            String timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            try
            {
                File.AppendAllText(
                    _path,
                    $"[{timestamp}] {content}{Environment.NewLine}"
                );
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            mut.ReleaseMutex();
        }

        Cleaner.LogCreated(content);
    }
}