//Overview: Executes on all the rules made by SortAlgorithm

using Core.FileSystem;
using Helper.FileSystem;
using System.Data;
using System.Diagnostics;
using System.Text.Json;

namespace Core.Sorter
{
    internal class RuleExecuter
    {
        private RuleEditor myRuleEditor; 
        public RuleExecuter()
        {
            myRuleEditor = new RuleEditor();
            AllRules = RuleEditor.GetRules();
        }

        public void ExecuteRule(Rule rule, FileInfo currentFileInfo)
        {
            Debug.Assert(rule.ShouldMove(currentFileInfo));

            string fileName = currentFileInfo.Name;
            string fullStartPath = Path.Combine(rule.StartPath, fileName);
            string fullEndPath = Path.Combine(rule.EndPath, fileName);

            FileSystemManager.Move(fullStartPath, fullEndPath);
        }

        private static List<Rule> ShouldExecuteRules(
            List<Rule> allRules,
            string currentPath,
            FileInfo currentFileInfo)
        {
            List<Rule> ShouldExecute = new();
            for (int i = 0; i < allRules.Count; i++)
            {
                Rule rule = allRules[i];
                if(rule.StartPath != currentPath) { continue; }

                if(rule.ShouldMove(currentFileInfo))
                {
                    ShouldExecute.Add(rule);
                }
            }
            return ShouldExecute;
        }


        /// <summary>
        /// Checks a folders' contents for any file that complies with the rule.
        /// Executes on those files.
        /// </summary>
        /// <param name="rule">the rule to check</param>
        /// <param name="folder">the path of the folder to check</param>
        void ExecuteRuleOnFolder(Rule rule, string folder)
        {
            const int fileLimit = 100;
            List<FileInfo> files = FileUtility.GetFileInfoOfFolder(folder, fileLimit);

            foreach (FileInfo file in files)
            {
                if (rule.ShouldMove(file))
                {
                    ExecuteRule(rule, file);
                }
            }
        }

        public void LoopThroughRules(List<Rule>? rules, FileInfo file, string oldPath)
        {
            if(rules == null) return;

            List<Rule> ExecuteRules = ShouldExecuteRules(rules, file.FullName, file);

            foreach(Rule rule in ExecuteRules)
            {
                ExecuteRule(rule, file);
            }
        }

        public void OnFileMove(string NewFilePath, string OldFilePath)
        {
            LoopThroughRules(AllRules, new(NewFilePath), OldFilePath);
        }

        public void OnFileCreated(string NewFilePath)
        {
            LoopThroughRules(AllRules, new(NewFilePath), null);
        }

        static List<Rule>? AllRules {get; set;}

        public static PromptUser PromptUserInst;

        public void OnRuleMade(Rule rule)
        {
            AllRules = RuleEditor.GetRules();

            string startPath = rule.StartPath;
            string startDirectory = Path.GetPathRoot(startPath);
            ExecuteRuleOnFolder(rule, startDirectory);
            myRuleEditor.AddRule(rule);
        }
    }
}
