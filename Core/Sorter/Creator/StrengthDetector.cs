using Core.Sorter.MatchDetectors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Core.Sorter.Creator
{
    public class StrengthDetector
    {
        /// <returns>The strongest rules in a list of rules. 
        /// (Only plural if there are multiple rules with the same highest strength.)</returns>
        public List<Rule> StrongestRules(List<Rule> rules)
        {
            int LargestStrength = GetLargestStrength(rules);
            List<Rule> StrongestRules = new();

            StrongestRules = rules.Where(rule =>
            {
                return rule.Strength == LargestStrength;
            }).ToList();

            return StrongestRules;
        }


        /// <returns>The strength of the strongest rule in a list of rules</returns>
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

        /// <returns>The first strongest rule in a list of rules.</returns>
        public Rule? StrongestRule(List<Rule>? Rules)
        {
            Debug.Assert(Rules != null && Rules.Count > 0);

            int LargestStrength = StrengthDetector.GetLargestStrength(Rules);
            for (int i = 0; i < Rules.Count; i++)
            {
                if (Rules[i].Strength == LargestStrength)
                    return Rules[i];
            }

            return null;
        }

        /// <param name="requiredStrength">filter for strength</param>
        /// <param name="requiredParams">filter for paramater count</param>
        /// <param name="requiredMatches">filter for the ammount of certain matches</param>
        /// <param name="limit"></param>
        /// <returns>a list of rules that match or exceed all the filters</returns>
        public List<Rule> RemoveWeakRules(
            List<Rule> allRules,
            int requiredStrength = 10,
            int requiredParams = 2,
            int requiredMatches = 2,
            int limit = 1000)
        {
            List<Rule> strongRules = allRules.Where(rule =>
            {
                return rule.StartCertain >= requiredMatches &&
                       rule.Parameters.Count >= requiredParams &&
                       rule.Strength >= requiredStrength;
            }).ToList();

            return strongRules;
        }
    }
}
