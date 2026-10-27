using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Sorter.Test.Helpers
{
    public class MassFileCreator
    {
        public string FilePath { get; set; }
        private List<string> MyFiles { get; set; }
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
            MyFiles.AddRange(Files);
            List<FileStream> AllFiles = new();
            foreach(string filename in Files)
            {
                FileStream file = File.Create(Path.Combine(FilePath, filename));
                AllFiles.Add(file);
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
            DisposeAll(Create(MyFiles));
        }

        public void Delete(List<string> Files)
        {
            foreach (string filename in Files)
            {
                File.Delete(Path.Combine(FilePath, filename));
            }
        }

        public void Delete()
        {
            Delete(MyFiles);
        }
    }
}
