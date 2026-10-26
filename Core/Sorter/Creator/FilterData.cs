using Core.Sorter.MatchDetectors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using static Core.Sorter.Creator.FilterData;

namespace Core.Sorter.Creator
{
    /// <summary>
    /// Class to get rid of useless rule and fileinfo data.
    /// </summary>
    public class FilterData
    {
        private DatapointFile _DatapointFile;
        public FilterData()
        {
            string DatapointFilePath = Path.Combine(AppContext.BaseDirectory, $"PreviousDatapoints.json");
            _DatapointFile = new DatapointFile(DatapointFilePath, null);
            PreviousDatapoints = _DatapointFile.Get();
        }

        /// <summary>
        /// Filters for only files that moved to and from the same path
        /// as datapoint.
        /// </summary>
        /// <param name="data">all old data</param>
        /// <param name="datapoint">just the newest datapoint</param>
        /// <returns>The dataset minus extra datapoints.</returns>
        public List<FileInfo> Filter(List<FileInfo> data, FileInfo datapoint)
        {
            List<FileInfo> filtered = new List<FileInfo>();
            foreach (FileInfo filedatapoint in data)
            {
                if (!filedatapoint.Exists)
                { continue; }
                if (filedatapoint.DirectoryName != datapoint.DirectoryName)
                { continue; }

                filtered.Add(filedatapoint);
            }
            return filtered;
        }

        /// <summary>
        /// Makes sure that the rules are distint and that none of them exist
        /// in the rule file already.
        /// </summary>
        /// <param name="Rules"></param>
        /// <returns></returns>
        public List<MatchDetectors.Rule> DetectDuplicateRules(
            List<MatchDetectors.Rule> NewRules, 
            List<MatchDetectors.Rule> OldRules)
        {
            List<MatchDetectors.Rule> result = new();
            result = NewRules.Distinct().ToList();
            foreach (MatchDetectors.Rule oldRule in OldRules)
            {
                if (result.Contains(oldRule))
                {
                    result.Remove(oldRule);
                }
            }

            foreach (MatchDetectors.Rule oldRule in OldRules)
            {
                for (int i = result.Count - 1; i >= 0; i--)
                {
                    MatchDetectors.Rule newRule = result[i];
                    if (newRule.CheapRuleEqualityCheck(oldRule))
                    {
                        result.Remove(newRule);
                    }
                }
            }
            return result;
        }



        //NOTE: if tied for first, chooses the first strongest rule
        public MatchDetectors.Rule? StrongestRule(List<MatchDetectors.Rule>? Rules)
        {
            if (Rules == null) return null;

            int LargestStrength = StrengthDetector.GetLargestStrength(Rules);
            for (int i = 0; i < Rules.Count; i++)
            {
                if (Rules[i].Strength == LargestStrength)
                    return Rules[i];
            }

            return null;
        }

        public List<MatchDetectors.Rule> StrongRules(
            List<MatchDetectors.Rule> allRules, 
            int requiredStrength = 10, 
            int requiredParams = 2,
            int requiredMatches = 2,
            int limit = 1000)
        {
            List<MatchDetectors.Rule> strongRules = new();

            foreach (MatchDetectors.Rule rule in allRules)
            {
                int ruleStrength = rule.Strength;

                if(ruleStrength < requiredStrength)
                {
                    continue;
                }
                if(rule.Paramaters.Count < requiredParams)
                {
                    continue;
                }
                if (rule.StartCertain < requiredMatches)
                {
                    continue;
                }

                strongRules.Add(rule);
            }

            return strongRules;
        }

        /// <summary>
        /// How long to wait before prompting the user about a folder after they said no.
        /// </summary>
        private int promptAmmount = 10;
        private List<FilePromptDatapoint> PreviousDatapoints { get; set; }
        public bool RedundantData(FileInfo newDatapoint)
        {
            List<string> PreviousPrompts = FilePromptDatapoint.GetPromptedFiles(PreviousDatapoints);

            List<FilePromptDatapoint> Matches = PreviousDatapoints.FindAll(match =>
            {
                return match.File == newDatapoint.DirectoryName;
            });

            if((Matches.Count - PreviousPrompts.Count) > promptAmmount)
            {
                return true;
            }

            return false;
        }

        public class FilePromptDatapoint
        {
            public FilePromptDatapoint(string file, bool wasPrompted)
            {
                File = file;
                WasPrompted = wasPrompted;
            }

            public string File { get; set; }
            public bool WasPrompted { get; set; }

            public static List<string> GetPromptedFiles(List<FilePromptDatapoint> FromFilePrompts)
            {
                return FromFilePrompts.Where(filePrompt =>
                {
                    if (filePrompt.WasPrompted)
                    {
                        return true;
                    }
                    return false;
                }).Select(filePrompt =>
                {
                    return filePrompt.File;
                }).ToList();
            }
        }

        public void AddOldData(string newDatapoint, bool prompted)
        {
            FilePromptDatapoint NewData = new FilePromptDatapoint(newDatapoint, prompted);
            PreviousDatapoints.Add(NewData);
            _DatapointFile.Set(NewData);
        }
    }

    class DatapointFile
    {
        private static Mutex mut = new(false, "Global\\DatapointFileMutex");

        private string _path { get; set; }
        private List<FilePromptDatapoint> _data { get; set; }

        public DatapointFile(string path, List<FilePromptDatapoint>? data)
        {
            _path = path;
            EnsureFileExists();
            data ??= Get();
            _data = data;
        }

        private void EnsureFileExists()
        {
            if (!File.Exists(_path))
            {
                if (mut.WaitOne(10000))
                {
                    using (FileStream fs = File.Create(_path))
                    {
                        string d = JsonSerializer.Serialize(new List<object>());
                        byte[] info = new UTF8Encoding(true).GetBytes(d);
                        fs.Write(info, 0, info.Length);
                    }
                }
                mut.ReleaseMutex();
            }
        }

        public List<T> Get<T>()
        {
            List<T>? data = new();

            if (mut.WaitOne(1000))
            {
                using (StreamReader r = new StreamReader(_path))
                {
                    string d = r.ReadToEnd();
                    data = JsonSerializer.Deserialize<List<T>>(d);
                }
            }

            mut.ReleaseMutex();

            return data;
        }

        public List<FilePromptDatapoint> Get()
        {
            return Get<FilePromptDatapoint>();
        }

        public void Set<T>(List<T> data)
        {
            if (mut.WaitOne(1000))
            {
                using (StreamWriter r = new StreamWriter(_path))
                {
                    string json = JsonSerializer.Serialize(data);
                    r.Write(json);
                }
            }
            mut.ReleaseMutex();
        }

        public void Set(List<FilePromptDatapoint> data)
        {
            Set<FilePromptDatapoint>(data);
        }

        public void Set(FilePromptDatapoint datapoint)
        {
            _data.Add(datapoint);
            Set(_data);
        }
    }
}
