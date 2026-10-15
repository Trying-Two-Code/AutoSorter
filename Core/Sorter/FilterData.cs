using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Core.Sorter
{
    /// <summary>
    /// Class to get rid of useless rule and fileinfo data.
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

        /// <summary>
        /// Makes sure that the rules are distint and that none of them exist
        /// in the rule file already.
        /// </summary>
        /// <param name="Rules"></param>
        /// <returns></returns>
        public static List<Rule> DetectDuplicateRules(List<Rule> Rules)
        {
            //TODO: check that none of them exist in the rule file already
            return Rules.Distinct().ToList();
        }

        //NOTE: if tied for first, chooses the first strongest rule
        public static Rule? StrongestRule(List<Rule>? Rules)
        {
            if (Rules == null) return null;

            int LargestStrength = StrengthDetector.GetLargestStrength(Rules);
            Debug.WriteLine("Largest strength: " + LargestStrength);
            for (int i = 0; i < Rules.Count; i++)
            {
                if (Rules[i].strength() == LargestStrength)
                    return Rules[i];
            }

            return null;
        }

        public static List<Rule> StrongRules(List<Rule> allRules, int requiredStrength, int limit)
        {
            List<Rule> strongRules = new();

            foreach (Rule rule in allRules)
            {
                int ruleStrength = rule.strength();
                if(ruleStrength >= requiredStrength)
                {
                    strongRules.Add(rule);
                }
            }

            return strongRules;
        }
    }
}
