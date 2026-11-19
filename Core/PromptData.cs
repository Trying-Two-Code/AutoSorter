using Core.Sorter.MatchDetectors;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Sorter.Test;

namespace Core
{
    public class PromptData : EventArgs
    {
        required public string PromptString { get; set; }
        required public string ParameterInfos { get; set; }
        required public List<string> FileMoveInfos { get; set; }
        required public List<Rule> OldRules { private get; init; }
        required public string StartPath { get; set; }
        required public string EndPath { get; set; }
        public TestData? TestData { get; set; } = null;

        public List<string> OldRulesData
        {
            get
            {
                List<string> OldRuleData = new();
                int OldRuleIndex = 0;
                OldRuleData.Add("List of all old rules:\n");
                if (OldRules.Count == 0)
                {
                    OldRuleData.Add("[empty]");
                }
                foreach (Rule OldRule in OldRules)
                {
                    OldRuleIndex++;
                    OldRuleData.Add($"Rule {OldRuleIndex}: " +
                        $"\n- start path:\n- - {OldRule.StartPath}" +
                        $"\n- end path: \n- - {OldRule.EndPath}" +
                        $"\n- file matches: \n- - {OldRule.StartCertain}\n\n");
                }
                return OldRuleData;
            } }

        public PromptData defaultPrompt { get
            {
                return new PromptData()
                {
                    PromptString = "Create a rule? No info provided.",
                    ParameterInfos = "",
                    FileMoveInfos = new(),
                    OldRules = new(),
                    StartPath = "",
                    EndPath = ""
                };
            } }

        public static PromptData nullPrompt
        {
            get
            {
                return new PromptData()
                {
                    FileMoveInfos = null,
                    PromptString = null,
                    ParameterInfos = null,
                    OldRules = null,
                    StartPath = null,
                    EndPath = null
                };
            }
        }
    }
}
