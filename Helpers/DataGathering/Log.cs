using System.Diagnostics;

namespace Helper.DataGathering;

public static class Log
{
    private static readonly String _path = "./data/log.txt";

    public static void AppendLog(String content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);

        String timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        try
        {
            File.AppendAllText(
                _path,
                $"[{timestamp}] {content}{Environment.NewLine}"
            );
        }
        catch ( Exception ex ) 
        {
            Debug.WriteLine( ex );
        }
    }
}