using Core.PromptUser;
using Core.Sorter.MatchDetectors;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core.Sorter.Creator
{
    /// <summary>
    /// Sends prompts to the user via a UITask.
    /// </summary>
    public class PromptUser
    {
        public UITask UIEvent;

        public PromptUser(UITask _uIEvent)
        {
            UIEvent = _uIEvent;
        }

        private TimeSpan promptAmmount = new(0, 0, 30);
        private DateTime lastCheck = DateTime.MinValue;
        private bool isSpam { get
            {
                return (DateTime.Now - lastCheck) < promptAmmount;
            } }

        public async Task<PromptResponse> Prompt(Rule rule, List<Rule> oldRules, PromptData.PromptReason reason)
        {
            if (Mute) return PromptResponse.MutedPrompt;
            if (isSpam) return PromptResponse.SpamPrompt;
            lastCheck = DateTime.Now;

            PromptData data = GeneratePromptData(rule, oldRules, reason);

            return await Prompt(data);
        }

        public async Task<PromptResponse> Prompt(PromptData promptData)
        {
            Task<PromptResponse> uiTask = UIEvent(this, promptData);
            PromptResponse result = await uiTask;

            return result;
        }

        public PromptData GeneratePromptData(Rule rule, List<Rule> oldRules, PromptData.PromptReason reason)
        {
            string folderPath = $"Would you like to make a rule that goes from " +
                $"{rule.StartPath} to {rule.EndPath}? ";
            string StrengthInfo = $"It is estimated that similar actions were done (manually) up to " +
                $"{rule.StartCertain} times already. ";
            string ParameterInfo = $"The rule contains {rule?.Parameters?.Count ?? 0} parameters.\n\n";
            List<string> FileMoveInfos = new();

            if(rule.Parameters != null)
            {
                foreach (Param param in rule.Parameters)
                {
                    ParameterInfo +=
                        $"{param.FileProperty} must be " +
                        $"{param.PropertyContains}.\n";
                }
            }

            FileMoveInfos.Add($"The following files support the creation of this rule:\n\n");
            if(rule.MatchingFiles != null)
            {
                foreach (string matchingFile in rule.MatchingFiles)
                {
                    FileMoveInfos.Add($"- {matchingFile}");
                }
            }


            PromptData newData = new()
            {
                MyPromptReason = reason,
                PromptString = folderPath + StrengthInfo,
                ParameterInfos = ParameterInfo,
                FileMoveInfos = FileMoveInfos,
                OldRules = oldRules,
                StartPath = rule.StartPath,
                EndPath = rule.EndPath
            };

            return newData;
        }

        public bool Mute { get; set; }
    }
}
