using Core.Sorter.MatchDetectors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Core.Sorter.Creator
{
    public class StrengthDetector
    {
        public List<Rule> StrongestRules(List<Rule> rules)
        {
            int LargestStrength = GetLargestStrength(rules);
            List<Rule> StrongestRules = new();

            foreach (Rule rule in rules)
            {
                if (rule.Strength == LargestStrength) 
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
                int strength = Rules[i].Strength;
                if (strength > largestStrength)
                    largestStrength = strength;
            }
            return largestStrength;
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
            List<MatchDetectors.Rule> strongRules = allRules.Where(rule =>
            {
                return  rule.StartCertain >= requiredMatches &&
                        rule.Paramaters.Count >= requiredParams &&
                        rule.Strength >= requiredStrength;
            }).ToList();

            return strongRules;
        }
    }
}
