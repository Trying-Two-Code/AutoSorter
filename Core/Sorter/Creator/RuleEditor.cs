using Core.Sorter.MatchDetectors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Core.Sorter.Creator
{
    class RuleEditor
    {
        readonly static string RuleDataPath = @"Sorter/Rule.json";
        readonly static JSONDataMinipulator<Rule> JSONMinipulator = new(RuleDataPath, "RuleEditor");

        public void AddRule(Rule rule)
        {
            EditRule(rule, add: true, remove: false, save: true);
        }

        public void RemoveRule(Rule rule)
        {
            EditRule(rule, remove: true, add: false, save: true);
        }

        public static async Task<List<Rule>?> EditRule(
            Rule? rule = null,
            bool remove = false,
            bool add = false,
            bool save = true)
        {
            Debug.Assert(!(add && remove));

            if (rule == null)
                return null;

            List<Rule> data = GetRules();

            if (data != null)
            {
                if (add)
                    data.Add(rule);
                if (data.Contains(rule) && remove)
                    data.Remove(rule);
                if (save)
                    SaveRules(data);
            }
            return data;
        }

        public static List<Rule> GetRules()
        {
            return JSONMinipulator.Get();
        }

        public static void SaveRules(List<Rule> newData)
        {
            JSONMinipulator.Set(newData);
        }
    }
}
