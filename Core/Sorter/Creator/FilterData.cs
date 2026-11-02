using Core.Sorter.MatchDetectors;
using System;
using System.Collections.Generic;
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
        private JSONDataMinipulator<FilePromptDatapoint> _DatapointFile;
        public FilterData()
        {
            _DatapointFile = new JSONDataMinipulator<FilePromptDatapoint>("PreviousDatapoints.json", "DatapointFileMutex");
            PreviousDatapoints = _DatapointFile.Get();
        }

        /// <summary>
        /// Filters for only files that moved to and from the same path
        /// as datapoint.
        /// </summary>
        /// <param name="data">all old data</param>
        /// <param name="datapoint">just the newest datapoint</param>
        /// <returns>The dataset minus extra datapoints.</returns>
        public HashSet<FileInfo> Filter(HashSet<FileInfo> data, FileInfo datapoint)
        {
            HashSet<FileInfo> filtered = new HashSet<FileInfo>();

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
            IEnumerable<Rule> result = NewRules.Distinct().Where(rule =>
            {
                return rule.CheapRuleEqualityCheck(OldRules.Distinct()).ToList().Count == 0;
            }).ToList();

            return result.ToList();
        }

        /// <summary>
        /// How long to wait before prompting the user about a folder after they said no.
        /// </summary>
        public int promptAmmount { get; set; } = 10;
        private List<FilePromptDatapoint> PreviousDatapoints { get; set; }
        public bool RedundantData(FileInfo newDatapoint)
        {
            List<string> PreviousPrompts = FilePromptDatapoint.GetPromptedFiles(PreviousDatapoints);

            if (PreviousPrompts.Count == 0)
                return promptAmmount >= 0;

            List<FilePromptDatapoint> Matches = PreviousDatapoints.Where(match =>
            {
                return Path.GetDirectoryName(match.File) == newDatapoint.DirectoryName;
            }).ToList();

            for (int i = PreviousDatapoints.Count - 1; i >= 0; i--)
            {
                FilePromptDatapoint data = PreviousDatapoints[i];
                if (data.WasPrompted)
                {
                    //Negative index is how many falses preceded the true
                    //aka: how many prompts didn't happen before now
                    int NegativeIndex = PreviousDatapoints.Count - i; 
                    return NegativeIndex >= promptAmmount;
                }
            }

            // (if statement is implied)
            // if (matches.Count == 0)
            return promptAmmount >= 0;
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
}
