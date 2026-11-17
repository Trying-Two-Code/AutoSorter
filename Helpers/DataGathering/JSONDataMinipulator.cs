using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Core.Sorter
{
    public class JSONDataMinipulator<ClassType> : IDisposable
    {
        private readonly Mutex Mut;
        public string MyPath { get; set; }
        public bool Active { get; set; }
        public List<ClassType> Data;

        public JSONDataMinipulator(string _path, string _MutexIdentity, List<ClassType>? _Data = null)
        {
            MyPath = Path.Combine(AppContext.BaseDirectory, _path);
            Active = true;

            Mut = new(false, $"Global\\{_MutexIdentity}");

            EnsureFileExists();

            _Data ??= Get();
            Data = _Data;
        }

        private void EnsureFileExists()
        {
            bool ExistsAndPopulated = File.Exists(MyPath) && new FileInfo(MyPath).Length != 0;

            if (!ExistsAndPopulated)
            {
                if (Mut.WaitOne(10000))
                {
                    try
                    {
                        using (FileStream fs = File.Create(MyPath))
                        {
                            string d = JsonSerializer.Serialize(new List<ClassType>());
                            byte[] info = new UTF8Encoding(true).GetBytes(d);
                            fs.Write(info, 0, info.Length);
                        }
                    } 
                    catch (FileNotFoundException)
                    {
                        File.Create(MyPath);
                    }
                    catch (DirectoryNotFoundException)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(MyPath)!);
                    }
                    finally
                    {
                        Mut.ReleaseMutex();
                    }
                }
            }
        }

        /// <summary>
        /// Overwrites the data file with an empty list. Called when the file
        /// exists but does not contain valid JSON for the expected type
        /// (for example a legacy "Rule.json" that holds "{}" instead of "[]").
        /// A bad data file should never take down the whole app.
        /// </summary>
        private void RepairMalformedFile()
        {
            try
            {
                File.WriteAllText(MyPath, "[]");

                Debug.WriteLine($"Repaired malformed JSON data file: {MyPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public List<T> Get<T>()
        {
            if (!File.Exists(MyPath))
                EnsureFileExists();

            if (Mut.WaitOne(1000))
            {
                try
                {
                    using (StreamReader r = new StreamReader(MyPath))
                    {
                        string d = r.ReadToEnd();

                        if (string.IsNullOrWhiteSpace(d))
                            return new();

                        List<T>? data = JsonSerializer.Deserialize<List<T>>(d) ?? new();
                        return data;
                    }
                }
                catch (JsonException e)
                {
                    Debug.WriteLine(e);

                    // The file exists but is not valid JSON for the expected
                    // type (e.g. "Rule.json" holding "{}" instead of "[]").
                    // Repair it so the rest of the app can keep running.
                    RepairMalformedFile();

                    return new();
                }
                finally
                {
                    Mut.ReleaseMutex();
                }
            }

            return new();
        }

        public List<ClassType> Get()
        {
            return Get<ClassType>();
        }

        public void Set<T>(List<T> data)
        {
            if (Mut.WaitOne(1000))
            {
                try
                {
                    using (StreamWriter r = new StreamWriter(MyPath))
                    {
                        string json = JsonSerializer.Serialize(data);
                        r.Write(json);
                    }
                }
                catch (JsonException)
                {
                    RepairMalformedFile();
                }
                finally
                {
                    Mut.ReleaseMutex();
                }
            }
        }

        public void Set(List<ClassType> data)
        {
            Set<ClassType>(data);
        }

        public void Set(ClassType datapoint)
        {
            Data.Add(datapoint);
            Set(Data);
        }

        public void Dispose()
        {
            Mut.Dispose();
        }
    }
}