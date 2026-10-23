//Overview: Executes on all the rules made by SortAlgorithm

using Core.FileSystem;
using Core.Sorter.Creator;
using Core.Sorter.MatchDetectors;
using Helper.FileSystem;
using System.Data;
using System.Diagnostics;
using System.Text.Json;

namespace Core.Sorter.Executor
{
    public class RuleExecuter
    {
        private RuleEditor myRuleEditor; 
        public RuleExecuter()
        {
            myRuleEditor = new RuleEditor();
            AllRules = RuleEditor.GetRules();
        }

        public void ExecuteRule(MatchDetectors.Rule rule, FileInfo currentFileInfo)
        {
            Debug.Assert(rule.ShouldMove(currentFileInfo));

            string fileName = currentFileInfo.Name;
            string fullStartPath = Path.Combine(rule.StartPath, fileName);
            string fullEndPath = Path.Combine(rule.EndPath, fileName);

            FileSystemManager.Move(fullStartPath, fullEndPath);

            rule.ExecutionCount++;
        }

        private static List<MatchDetectors.Rule> ShouldExecuteRules(
            List<MatchDetectors.Rule> allRules,
            string currentPath,
            FileInfo currentFileInfo)
        {
            List<MatchDetectors.Rule> ShouldExecute = new();
            for (int i = 0; i < allRules.Count; i++)
            {
                MatchDetectors.Rule rule = allRules[i];
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
        void ExecuteRuleOnFolder(MatchDetectors.Rule rule, string folder)
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

        public void LoopThroughRules(List<MatchDetectors.Rule>? rules, FileInfo file, string? oldPath)
        {
            if(rules == null) return;

            List<MatchDetectors.Rule> ExecuteRules = ShouldExecuteRules(rules, file.FullName, file);

            //issue, what if ExecuteRules.Count > 1
            foreach(MatchDetectors.Rule rule in ExecuteRules)
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

        static List<MatchDetectors.Rule>? AllRules {get; set;}

        public void OnRuleMade(MatchDetectors.Rule rule)
        {
            AllRules = RuleEditor.GetRules();

            string startPath = rule.StartPath;
            string? startDirectory = Path.GetPathRoot(startPath);
            if (startDirectory != null) {
                ExecuteRuleOnFolder(rule, startDirectory);
            }

            myRuleEditor.AddRule(rule);
        }
    }
}
