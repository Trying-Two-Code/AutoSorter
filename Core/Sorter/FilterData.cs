using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Core.Sorter
{
    /// <summary>
    /// Class to get rid of useless data.
    /// </summary>
    class FilterData()
    {
        /// <summary>
        /// Filters for only files that moved to and from the same path
        /// as datapoint.
        /// </summary>
        /// <param name="data">all old data</param>
        /// <param name="datapoint">just the newest datapoint</param>
        /// <returns>The dataset minus extra datapoints.</returns>
        public static List<FileInfo> Filter(List<FileInfo> data, FileInfo datapoint)
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

        public static List<Rule>? DetectDuplicateRules(List<Rule>? Rules)
        {
            if (Rules == null)
                return null;

            return Rules.Distinct()?.ToList();
        }

        //NOTE: if tied for first, chooses the first strongest rule
        public static Rule? StrongestRule(List<Rule>? Rules)
        {
            if (Rules == null) return null;

            int GetLargestStrength(List<Rule> Rules)
            {
                int largestStrength = 0;
                for (int i = 0; i < Rules.Count; i++)
                {
                    if (Rules[i].Strength > largestStrength)
                        largestStrength = Rules[i].Strength;
                }
                return largestStrength;
            }

            int LargestStrength = GetLargestStrength(Rules);
            for (int i = 0; i < Rules.Count; i++)
            {
                if (Rules[i].Strength == LargestStrength)
                    return Rules[i];
            }

            return null;
        }

        public static List<Rule> StrongRules(List<Rule> allRules, int requiredStrength, int limit)
        {
            List<Rule> strongRules = new();

            foreach (Rule ParameterCombination in allRules)
            {
                int ruleStrength = GetNewRuleStrength(ParameterCombination, limit);
                if(ruleStrength >= requiredStrength)
                {
                    strongRules.Add(ParameterCombination);
                }
            }

            return strongRules;
        }

        public static int GetNewRuleStrength(Rule rule, int limit)
        {
            string endPath = rule.EndPath;
            int strength = 0;

            IEnumerable<string> files = Directory.EnumerateFiles(endPath).Take(limit);

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (rule.MatchesParameters(fileInfo))
                {
                    strength += 1;
                }
            }

            return strength;
        }
    }
}
