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

        public class PromptData : EventArgs
        {
            public string PromptString { get; set; }
            public string ExtraInfo { get; set; }
        }

        private TimeSpan promptAmmount = new(0, 0, 30);
        private DateTime lastCheck = DateTime.MinValue;
        private bool isSpam { get
            {
                return (lastCheck - DateTime.Now) < promptAmmount;
            } }

        public bool Prompt(Rule rule)
        {
            if (Mute) return false;
            if (isSpam) return false;
            lastCheck = DateTime.Now;

            PromptData data = GeneratePromptData(rule);

            Task<bool> uiTask = UIEvent(this, data);
            Task<bool> uiResult = Task<bool>.Factory.StartNew(() =>
            {
                return uiTask.Result;
            });
            bool result = uiResult.Result;

            return result;
        }

        public PromptData GeneratePromptData(Rule rule)
        {
            string folderPath = $"Would you like to make a rule that goes from " +
                $"{rule.StartPath} to {rule.EndPath}? ";
            string StrengthInfo = $"It is estimated that similar actions were done (manually) up to " +
                $"{rule.StartEstimation} times already. ";
            string ParameterInfo = $"The rule contains ${rule.Parameters.Count} parameters. ";

            foreach(Param param in rule.Parameters)
            {
                ParameterInfo += 
                    $"{param.FileProperty} must be" +
                    $"{param.PropertyContains}.";
            }

            PromptData newData = new()
            {
                PromptString = folderPath + StrengthInfo,
                ExtraInfo = ParameterInfo
            };

            return newData;
        }

        public bool Mute { get; set; }
    }
}
