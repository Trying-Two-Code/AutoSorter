using Core.Sorter.MatchDetectors;
using System.Diagnostics;

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

        public async Task<bool> Prompt(Rule rule, List<Rule> oldRules)
        {
            if (Mute) return false;
            if (isSpam) return false;
            lastCheck = DateTime.Now;

            PromptData data = GeneratePromptData(rule, oldRules);

            Task<bool> uiTask = UIEvent(this, data);
            bool result = await uiTask;

            return result;
        }

        public PromptData GeneratePromptData(Rule rule, List<Rule> oldRules)
        {
            string folderPath = $"Would you like to make a rule that goes from " +
                $"{rule.StartPath} to {rule.EndPath}? ";
            string StrengthInfo = $"It is estimated that similar actions were done (manually) up to " +
                $"{rule.StartCertain} times already. ";
            string ParameterInfo = $"The rule contains ${rule?.Parameters?.Count ?? 0} parameters.\n\n";
            List<string> FileMoveInfos = new();

            if(rule.Parameters != null)
            {
                foreach (Param param in rule.Parameters)
                {
                    ParameterInfo +=
                        $"{param.FileProperty} must be" +
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
                PromptString = folderPath + StrengthInfo,
                ParameterInfos = ParameterInfo,
                FileMoveInfos = FileMoveInfos,
                OldRules = oldRules
            };

            return newData;
        }

        public bool Mute { get; set; }
    }
}
