using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                    using (FileStream fs = File.Create(MyPath))
                    {
                        string d = JsonSerializer.Serialize(new List<ClassType>());
                        byte[] info = new UTF8Encoding(true).GetBytes(d);
                        fs.Write(info, 0, info.Length);
                    }
                    Mut.ReleaseMutex();
                }
            }
        }

        public List<T> Get<T>()
        {
            //try
            //{
            if (Mut.WaitOne(1000))
            {
                Debug.WriteLine("MY PATH:" + MyPath);
                using (StreamReader r = new StreamReader(MyPath))
                {
                    string d = r.ReadToEnd();
                    List<T>? data = JsonSerializer.Deserialize<List<T>>(d) ?? new();
                    Mut.ReleaseMutex();
                    return data;
                }
            }
            /*}
            catch (Exception e)
            {
                Debug.WriteLine(e);
                EnsureFileExists();
            }*/
            
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
                using (StreamWriter r = new StreamWriter(MyPath))
                {
                    string json = JsonSerializer.Serialize(data);
                    r.Write(json);
                }
                Mut.ReleaseMutex();
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