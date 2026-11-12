using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class PromptData : EventArgs
    {
        required public string PromptString { get; set; }
        required public string ParameterInfos { get; set; }
        required public string FileMoveInfos { get; set; }

        public PromptData defaultPrompt { get
            {
                return new PromptData()
                {
                    PromptString = "Create a rule? No info provided.",
                    ParameterInfos = "",
                    FileMoveInfos = ""
                };
            } }
    }
}
