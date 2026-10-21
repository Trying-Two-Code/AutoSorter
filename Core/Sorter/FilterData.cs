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
    public class FilterData
    {
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
        public List<Rule> DetectDuplicateRules(List<Rule> NewRules, List<Rule> OldRules)
        {
            List<Rule> result = new();
            result = NewRules.Distinct().ToList();
            foreach (Rule oldRule in OldRules)
            {
                if (result.Contains(oldRule))
                {
                    result.Remove(oldRule);
                }
            }

            foreach (Rule oldRule in OldRules)
            {
                for (int i = result.Count - 1; i >= 0; i--)
                {
                    Rule newRule = result[i];
                    if (newRule.CheapRuleEqualityCheck(oldRule))
                    {
                        result.Remove(newRule);
                    }
                }
            }
            return result;
        }



        //NOTE: if tied for first, chooses the first strongest rule
        public Rule? StrongestRule(List<Rule>? Rules)
        {
            if (Rules == null) return null;

            int LargestStrength = StrengthDetector.GetLargestStrength(Rules);
            for (int i = 0; i < Rules.Count; i++)
            {
                if (Rules[i].strength() == LargestStrength)
                    return Rules[i];
            }

            return null;
        }

        public List<Rule> StrongRules(
            List<Rule> allRules, 
            int requiredStrength = 10, 
            int requiredParams = 2,
            int requiredMatches = 10, 
            int limit = 1000)
        {
            List<Rule> strongRules = new();

            foreach (Rule rule in allRules)
            {
                int ruleStrength = rule.strength();

                if(ruleStrength < requiredStrength)
                {
                    continue;
                }
                if(rule.Paramaters.Count < requiredParams)
                {
                    continue;
                }
                if (rule.StartEstimation < requiredMatches)
                {
                    continue;
                }

                strongRules.Add(rule);
            }

            return strongRules;
        }
    }
}
