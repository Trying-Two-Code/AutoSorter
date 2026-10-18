using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Core.Sorter
{
    class RuleEditor
    {
        public static object _lock = new();
        readonly static string RuleDataPath = @"Sorter/Rule.json";

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
            List<Rule> data = new();

            lock (_lock)
            {
                using (StreamReader r = new StreamReader(RuleDataPath))
                {
                    string json = r.ReadToEnd();
                    List<Rule>? jsonData = JsonSerializer.Deserialize<List<Rule>>(json);
                    if(jsonData != null)
                        data = jsonData;
                }
            }

            return data;
        }

        public static void SaveRules(List<Rule> newData)
        {
            lock (_lock)
            {
                using (StreamWriter r = new StreamWriter(RuleDataPath))
                {
                    string json = JsonSerializer.Serialize(newData);
                    r.Write(json);
                }
            }
        }
    }
}
