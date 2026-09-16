//Overview: Executes on all the rules made by SortAlgorithm

using Core.FileSystem;
using System.Diagnostics;

namespace Core.Sorter
{
    internal class RuleExecuter
    {
        public static void ExecuteRule(Rule rule, string currentPath)
        {
            FileInfo fileInfo = new FileInfo(currentPath);

            Debug.Assert(rule.ShouldMove(fileInfo, currentPath));
        }

        private static Rule? ShouldExecuteRule(List<Rule> allRules, string currentPath)
        {
            FileInfo? currentFileInfo = null;

            for (int i = 0; i < allRules.Count; i++)
            {
                Rule rule = allRules[i];
                if(rule.StartPath != currentPath) { continue; }

                currentFileInfo ??= new FileInfo(currentPath);

                if(rule.ShouldMove(currentFileInfo, currentPath))
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
                if (rule.ShouldMove(fileInfo, file))
                {
                    ExecuteRule(rule, file);
                }
            }
        }

        void LoopThroughRules(List<Rule>? rules)
        {
            if(rules == null) return;
            //for rule in rules:
            //    if ShouldExecuteRule(rule):
            //        ExecuteRule(rule)
        }

        void OnFileMove(object sender, OnFileMoveEventArgs e)
        {
            LoopThroughRules(AllRules);
        }

        void OnFileCreated(object sender, OnFileMoveEventArgs e)
        {
            LoopThroughRules(AllRules);
        }

        static List<Rule>? AllRules {get; set;} = new List<Rule>();
        static void OnRuleMade(object sender, OnRuleMadeEventArgs e)
        {
            if(AllRules == null)
            ExecuteRuleOnFolder(e.rule, e.rule.StartPath);
        }
    }
}
