using App.Ui.PromptUser.Visuals;
using App.Ui.UiScripts;
using Core;
using Core.PromptUser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Threading;

namespace App.Ui.PromptUser
{
    public class UserPrompter
    {
        private RuleMadePromptWindow? MyPromptWindow;
        private readonly TimeSpan promptAmmount = new TimeSpan(0, 0, 10);
        private DateTime lastPromptTime = DateTime.MinValue;
        private Settings mySettings;
        private Dispatcher myUiDispatcher;
        private PromptUserTest myTest = new();

        public UserPrompter(Settings _settings, Dispatcher dispatcher)
        {
            mySettings = _settings;
            myUiDispatcher = dispatcher;
        }

        /// <summary>
        /// Translates prompts into a resulting method that
        /// matches the reason the prompt was sent here.
        /// </summary>
        public async Task<PromptResponse> PromptUser(PromptData promptData)
        {
            Debug.WriteLine("prompting user now.");
            lastPromptTime = DateTime.Now;

            switch (promptData.MyPromptReason) 
            {
                case PromptData.PromptReason.Test:
                    return await TestPromptUser(promptData);
                case PromptData.PromptReason.RuleCreated:
                    return await CreateRuleCreatedPrompt(promptData);
                case PromptData.PromptReason.RuleExecuted:
                    return await CreateRuleExecutedPrompt(promptData);
                default:
                    return await TestPromptUser(promptData);
            }
        }

        public async Task<PromptResponse> TestPromptUser(PromptData promptData)
        {
            RuleMadePromptWindow? TestPromptWindow = null;
            Task<PromptResponse>? promptUserTestTask = null;

            await myUiDispatcher.InvokeAsync(() =>
            {
                TestPromptWindow = new RuleMadePromptWindow("Test Window", promptData, mySettings);
                TestPromptWindow.Show();

                promptUserTestTask = myTest.TestPrompt(promptData, TestPromptWindow);

                return promptUserTestTask;
            });

            return await promptUserTestTask!;
        }

        public async Task<PromptResponse> CreateRuleCreatedPrompt(PromptData promptData)
        {
            RuleMadePromptWindow? MyPromptWindow = null;

            await myUiDispatcher.InvokeAsync(() =>
            {
                MyPromptWindow = new RuleMadePromptWindow("Create Rule?", promptData, mySettings);
                MyPromptWindow.Show();
            });

            return await MyPromptWindow!.GetUserInput();
        }

        public async Task<PromptResponse> CreateRuleExecutedPrompt(PromptData promptData)
        {
            RuleExecutedPromptWindow? MyPromptWindow = null;

            await myUiDispatcher.InvokeAsync(() =>
            {
                MyPromptWindow = new RuleExecutedPromptWindow(promptData);
                MyPromptWindow.Show();
            });

            return await MyPromptWindow!.GetUserInput();
        }

        public Task<PromptResponse> FalseResult = Task.Run(() =>
            {
                return PromptResponse.NeverPrompted;
            });

        public async Task<PromptResponse> HandleRequest(object sender, PromptData args)
        {
            if(args.TestData == null)
            {
                if (DateTime.Now - lastPromptTime < promptAmmount)
                {
                    return await FalseResult;
                }
            }

            if (args != null)
                return await PromptUser(args);
            else
                return await PromptUser(PromptData.nullPrompt);
        }
    }
}
