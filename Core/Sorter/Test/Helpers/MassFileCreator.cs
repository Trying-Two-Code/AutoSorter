using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Core.Sorter.Test.Helpers
{
    public class MassFileCreator
    {
        public string FilePath { get; private set; }
        public List<string> MyFiles { get; private set; } = new();

        public List<FileInfo> MyFileInfos { get {
                return MyFiles.Select(file =>
                {
                    return new FileInfo(file);
                }).ToList();
            } }

        public MassFileCreator(string _FilePath)
        {
            FilePath = _FilePath;
        }

        public List<FileStream> Create(List<string> Files)
        {
            List<FileStream> AllFiles = new();
            foreach(string filename in Files)
            {
                string path = Path.Combine(FilePath, filename);
                FileStream file = File.Create(path);
                AllFiles.Add(file);
                MyFiles.Add(file.Name);
            }
            return AllFiles;
        }

        public void DisposeAll(List<FileStream> FileStreams)
        {
            foreach(FileStream file in FileStreams)
            {
                file.Dispose();
            }
        }

        public void CreateAndForget(List<string> Files)
        {
            DisposeAll(Create(Files));
        }

        public void Delete(List<string> Files)
        {
            for (int i = Files.Count - 1; i >= 0; i--)
            {
                string filename = Files[i];
                File.Delete(Path.Combine(FilePath, filename));
                MyFiles.Remove(filename);
            }
        }

        public void Delete()
        {
            Delete(MyFiles);
        }

    }
}
