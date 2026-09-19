using System;
using System.Collections.Generic;
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

        public static List<Rule>? DetectDuplicateRules(List<Rule> Rules)
        {
            return null;
        }

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
    }
}
