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
}
