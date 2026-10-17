using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Sorter
{
    public class StrengthDetector
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

        public List<FileInfo> RuleMatches(Rule rule, string folderPath)
        {
            List<FileInfo> FilesInFolder = FileUtility.GetFileInfoOfFolder(folderPath);
            return rule.Matches(FilesInFolder);
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
