using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Core.Sorter
{
    class JSONDataMinipulator<ClassType>
    {
        private static Mutex Mut;
        public string Path { get; set; }
        public bool Active { get; set; }
        public List<ClassType> Data;

        public JSONDataMinipulator(string _path, string _MutexIdentity, List<ClassType>? _Data = null)
        {
            Path = _path;
            Active = true;

            Mut = new(false, $"Global\\{_MutexIdentity}");

            EnsureFileExists();

            _Data ??= Get();
            Data = _Data;
        }

        private void EnsureFileExists()
        {
            if (!File.Exists(Path))
            {
                if (Mut.WaitOne(10000))
                {
                    using (FileStream fs = File.Create(Path))
                    {
                        string d = JsonSerializer.Serialize(new List<ClassType>());
                        byte[] info = new UTF8Encoding(true).GetBytes(d);
                        fs.Write(info, 0, info.Length);
                    }
                }
                Mut.ReleaseMutex();
            }
        }

        public List<T> Get<T>()
        {
            List<T>? data = new();

            if (Mut.WaitOne(1000))
            {
                using (StreamReader r = new StreamReader(Path))
                {
                    string d = r.ReadToEnd();
                    data = JsonSerializer.Deserialize<List<T>>(d);
                }
            }

            Mut.ReleaseMutex();

            return data;
        }

        public List<ClassType> Get()
        {
            return Get<ClassType>();
        }

        public void Set<T>(List<T> data)
        {
            if (Mut.WaitOne(1000))
            {
                using (StreamWriter r = new StreamWriter(Path))
                {
                    string json = JsonSerializer.Serialize(data);
                    r.Write(json);
                }
            }
            Mut.ReleaseMutex();
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
    }
}
