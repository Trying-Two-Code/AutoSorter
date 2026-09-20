//Overview: Executes on all the rules made by SortAlgorithm

using Core.FileSystem;
using Helper.FileSystem;
using System.Diagnostics;
using System.Text.Json;

namespace Core.Sorter
{
    class RuleEditor
    {
        readonly static string RuleDataPath = @"Sorter/Rule.json";

        public static void AddRule(Rule rule)
        {
            EditRule(rule, add: true, save: true);
        }

        public static void RemoveRule(Rule rule) 
        {
            EditRule(rule, remove: true, save: true);
        }

        public static List<Rule>? EditRule(
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
                if(add)
                    data.Add(rule);
                if (data.Contains(rule) && remove)
                    data.Remove(rule);
                if(save)
                    SaveRules(data);
            }
            return data;
        }

        public static List<Rule> GetRules()
        {
            List<Rule> data = new();

            try
            {
                using (StreamReader r = new StreamReader(RuleDataPath))
                {

                    string json = r.ReadToEnd();
                    List<Rule> jsonData = JsonSerializer.Deserialize<List<Rule>>(json);
                    data = jsonData;
                }
            }
            catch
            {
                //json is not valid, must be reset
                SaveRules(data);
            }

            return data;
        }

        public static void SaveRules(List<Rule> newData)
        {
            using(StreamWriter r = new StreamWriter(RuleDataPath))
            {
                string json = JsonSerializer.Serialize(newData);
                r.Write(json);
            }
        }
    }

    internal class RuleExecuter
    {
        public static void ExecuteRule(Rule rule, FileInfo currentFileInfo)
        {
            Debug.Assert(rule.ShouldMove(currentFileInfo));

            string fileName = currentFileInfo.Name;
            string fullStartPath = rule.StartPath + fileName;
            string fullEndPath = rule.EndPath + fileName;

            FileSystemManager.Move(fullStartPath, fullEndPath);
        }

        private static Rule? ShouldExecuteRule(
            List<Rule> allRules, 
            string currentPath, 
            FileInfo currentFileInfo)
        {
            for (int i = 0; i < allRules.Count; i++)
            {
                Rule rule = allRules[i];
                if(rule.StartPath != currentPath) { continue; }

                if(rule.ShouldMove(currentFileInfo))
                {
                    return rule;
                }
            }
            return null;
        }

        /// <summary>
        /// Checks a folders' contents for any file that complies with the rule.
        /// Executes on those files.
        /// </summary>
        /// <param name="rule">the rule to check</param>
        /// <param name="folder">the path of the folder to check</param>
        static void ExecuteRuleOnFolder(Rule rule, string folder)
        {
            const int fileLimit = 100;
            IEnumerable<string> files = Directory.EnumerateFiles(folder).Take(fileLimit);

            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (rule.ShouldMove(fileInfo))
                {
                    ExecuteRule(rule, fileInfo);
                }
            }
        }

        public static void LoopThroughRules(List<Rule>? rules, FileInfo file)
        {
            if(rules == null) return;

            Rule? rule = ShouldExecuteRule(rules, file.FullName, file);

            if (rule != null)
            {
                ExecuteRule(rule, file);
            }
        }

        public static void OnFileMove(string NewFilePath)
        {
            LoopThroughRules(AllRules, new(NewFilePath));
        }

        public static void OnFileCreated(string NewFilePath)
        {
            LoopThroughRules(AllRules, new(NewFilePath));
        }

        static List<Rule>? AllRules {get; set;} = RuleEditor.GetRules();

        public static void OnRuleMade(Rule rule)
        {
            if(AllRules == null)
            ExecuteRuleOnFolder(rule, rule.StartPath);
            RuleEditor.AddRule(rule);
            AllRules = RuleEditor.GetRules();
        }
    }
}
