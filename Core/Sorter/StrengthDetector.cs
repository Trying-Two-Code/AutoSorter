using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Sorter
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
    }
}
