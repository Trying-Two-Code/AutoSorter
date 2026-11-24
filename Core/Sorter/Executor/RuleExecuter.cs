//Overview: Executes on all the rules made by SortAlgorithm

using Core.Sorter.Creator;
using Helper.FileSystem;
using Helpers.DataGathering;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using static Core.Sorter.Test.TestMatchDetectors;

namespace Core.Sorter.Executor
{
    public class RuleExecuter
    {
        private RuleEditor MyRuleEditor { get; set; }
        private PromptUser? MyPromptUser { get; set; }
        private JSONDataMinipulator<FileMoveInfo> MoveHistoryJSON;

        public RuleExecuter(PromptUser _promptUser)
        {
            MoveHistoryJSON = new JSONDataMinipulator<FileMoveInfo>("RuleExecutionHistory.json", "RuleExecutor");
            MyRuleEditor = new RuleEditor();
            AllRules = RuleEditor.GetRules();
            MyPromptUser = _promptUser;

            //TEST DETECT RULE:
            /*if (MyPromptUser != null && MyPromptUser.UIEvent != null)
            {
                Debug.WriteLine($"EXECUTING PROMPT: |{MyPromptUser.UIEvent}|");
                MatchDetectors.Rule testRule = new MatchDetectors.Rule()
                {
                    Parameters = new()
                    {
                        new("")
                    },
                    StartPath = "",
                    EndPath = ""
                };
                FileInfo testInfo = new FileInfo("C:\\NonExistant");
                DetectExecuteRule(testRule, testInfo);
            }*/
        }

        public async void DetectExecuteRule(MatchDetectors.Rule rule, FileInfo currentFileInfo)
        {
            PromptData data = new PromptData()
            {
                MyPromptReason = PromptData.PromptReason.RuleExecuted,
                PromptString = $"Rule executed on folder: {currentFileInfo.DirectoryName}",
                ParameterInfos = rule.Parameters.Select(
                    param => { return $"{param.FileProperty} contains: {param.PropertyContains}"; })
                    .ToString()!,
                FileMoveInfos = null,
                OldRules = AllRules,
                StartPath = rule.StartPath,
                EndPath = rule.EndPath
            };

            if(MyPromptUser == null)
            {
                throw new NoUserPrompterException();
            }

            bool promptResult = await MyPromptUser.Prompt(data);

            if (promptResult)
            {
                ExecuteRule(rule, currentFileInfo);
            }
        }

        private class NoUserPrompterException : Exception { }

        public async void ExecuteRule(MatchDetectors.Rule rule, FileInfo currentFileInfo)
        {
            Debug.Assert(rule.ShouldMove(currentFileInfo));

            string fileName = currentFileInfo.Name;
            string fullStartPath = Path.Combine(rule.StartPath, fileName);
            string fullEndPath = Path.Combine(rule.EndPath, fileName);

            MoveFile(fullStartPath, fullEndPath);

            rule.ExecutionCount++;

            FileMoveInfo ExecutedMove = new()
            {
                OldPath = fullStartPath,
                NewPath = fullEndPath
            };

            MoveHistoryJSON.Set(ExecutedMove);
        }

        public async void MoveFile(string start, string end)
        {
            await Task.Run(() =>
            {
                FileSystemManager.Move(start, end);
            });
        }

        public void UndoExecution(FileMoveInfo UndoMove)
        {
            Debug.Assert(File.Exists(UndoMove.NewPath));
            FileSystemManager.Move(UndoMove.NewPath, UndoMove.OldPath);
        }

        public void UndoExecution(List<FileMoveInfo> UndoMoves)
        {
            foreach(FileMoveInfo MoveInfo in UndoMoves)
            {
                UndoExecution(MoveInfo);
            }
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
        public void ExecuteRuleOnFolder(MatchDetectors.Rule rule, string folder)
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

        public async Task LoopThroughRules(List<MatchDetectors.Rule>? rules, FileInfo file, string? oldPath)
        {
            if(rules == null) return;

            List<MatchDetectors.Rule> ExecuteRules = ShouldExecuteRules(rules, file.FullName, file);

            //issue, what if ExecuteRules.Count > 1
            foreach(MatchDetectors.Rule rule in ExecuteRules)
            {
                try
                {
                    DetectExecuteRule(rule, file);
                } catch (NoUserPrompterException)
                {
                    Debug.WriteLine("There was no user prompter to prompt!");

                    Debug.WriteLine("Executing anyways.");
                    ExecuteRule(rule, file);
                }
            }
        }

        public void OnFileMove(string NewFilePath, string OldFilePath)
        {
            Task.Run(async () =>
            {
                await SafeLoopThroughRules(AllRules, new(NewFilePath), OldFilePath);
            });
        }

        public async Task SafeLoopThroughRules(List<MatchDetectors.Rule>? rules, FileInfo file, string? oldPath)
        {
            if (!Cleaner.IsClean(file.FullName))
            {
                return;
            }

            while (!file.Exists)
                await Task.Delay(100);

            await LoopThroughRules(rules, file, oldPath);
        }

        public void OnFileCreated(string NewFilePath)
        {
            SafeLoopThroughRules(AllRules, new(NewFilePath), null);
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

            MyRuleEditor.AddRule(rule);
        }
    }
}
