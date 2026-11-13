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

        public bool Prompt(Rule rule, List<Rule> oldRules)
        {
            if (Mute) return false;
            if (isSpam) return false;
            lastCheck = DateTime.Now;

            PromptData data = GeneratePromptData(rule, oldRules);

            Task<bool> uiTask = UIEvent(this, data);
            Task<bool> uiResult = Task<bool>.Factory.StartNew(() =>
            {
                return uiTask.Result;
            });
            bool result = uiResult.Result;

            return result;
        }

        public PromptData GeneratePromptData(Rule rule, List<Rule> oldRules)
        {
            string folderPath = $"Would you like to make a rule that goes from " +
                $"{rule.StartPath} to {rule.EndPath}? ";
            string StrengthInfo = $"It is estimated that similar actions were done (manually) up to " +
                $"{rule.StartCertain} times already. ";
            string ParameterInfo = $"The rule contains ${rule.Parameters.Count} parameters.\n\n";

            foreach(Param param in rule.Parameters)
            {
                ParameterInfo += 
                    $"{param.FileProperty} must be" +
                    $"{param.PropertyContains}.\n";
            }

            List<string> FileMoveInfos = new();
            FileMoveInfos.Add($"The following files support the creation of this rule:");

            foreach(string matchingFile in rule.MatchingFiles)
            {
                FileMoveInfos.Add($"- {matchingFile}");
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
