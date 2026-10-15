using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Sorter
{
    internal class StrengthDetector
    {
        public int? ParamStrength(
                Param detector,
                List<FileInfo> allData
            )
        {
            const int positiveMultiplier = 1;
            const int negativeMultiplier = 1;
            int positiveCount = 0;
            int negativeCount = 0;

            foreach (FileInfo datapoint in allData)
            {
                if (detector.IsMatch(datapoint))
                {
                    positiveCount++;
                    detector.MatchingFiles.Add(datapoint.FullName);
                }
                else
                {
                    negativeCount++;
                }
            }

            int positiveResult = positiveCount * positiveMultiplier;
            int negativeResult = negativeCount * negativeMultiplier;
            int result = positiveResult - negativeResult;

            return result;
        }

        /// <summary>
        /// Finds the file matches for a given rule for a list of files
        /// </summary>
        /// <param name="rule">the rule to find matches for</param>
        /// <param name="corralationFiles">the list of files</param>
        /// <returns>The list of files that match the rule</returns>
        public List<FileInfo> RuleMatches(Rule rule, List<FileInfo> corralationFiles)
        {
            foreach (Param param in rule.Paramaters)
            {
                for (int i = corralationFiles.Count - 1; i >= 0; i--)
                {
                    FileInfo fileInfo = corralationFiles[i];
                    if (!param.IsMatch(fileInfo))
                    {
                        corralationFiles.Remove(fileInfo);
                    }
                }
            }
            return corralationFiles;
        }

        public List<FileInfo> RuleMatches(Rule rule, string folderPath)
        {
            List<FileInfo> FilesInFolder = FileUtility.GetFileInfoOfFolder(folderPath);
            return RuleMatches(rule, FilesInFolder);
        }

        public List<Rule> StrongestRules(List<Rule> rules)
        {
            int LargestStrength = GetLargestStrength(rules);
            List<Rule> StrongestRules = new();

            foreach (Rule rule in rules)
            {
                if (rule.strength() == LargestStrength) 
                {
                    StrongestRules.Add(rule);
                }
            }

            return StrongestRules;
        }

        public static int GetLargestStrength(List<Rule> Rules)
        {
            int largestStrength = 0;
            for (int i = 0; i < Rules.Count; i++)
            {
                int strength = Rules[i].strength();
                if (strength > largestStrength)
                    largestStrength = strength;
            }
            return largestStrength;
        }
    }
}
