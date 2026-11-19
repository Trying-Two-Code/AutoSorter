using App.Ui.PromptUser.Visuals;
using App.Ui.UiScripts;
using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;

namespace App.Ui.PromptUser
{
    public class UserPrompter
    {
        private PromptWindow? MyPromptWindow;
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

        public async Task<bool> PromptUser(PromptData promptData)
        {
            lastPromptTime = DateTime.Now;

            if (promptData.TestData != null)
            {
                return await TestPromptUser(promptData);
            }
            else
            {
                return await CreatePrompt(promptData);
            }
        }

        public async Task<bool> TestPromptUser(PromptData promptData)
        {
            PromptWindow? TestPromptWindow = null;
            Task<bool>? promptUserTestTask = null;

            await myUiDispatcher.InvokeAsync(() =>
            {
                TestPromptWindow = new PromptWindow("Test Window", promptData, mySettings);
                TestPromptWindow.Show();

                promptUserTestTask = myTest.TestPrompt(promptData, TestPromptWindow);

                return promptUserTestTask;
            });

            return await promptUserTestTask!;
        }

        public async Task<bool> CreatePrompt(PromptData promptData)
        {
            PromptWindow? MyPromptWindow = null;

            await myUiDispatcher.InvokeAsync(() =>
            {
                MyPromptWindow = new PromptWindow("Create Rule?", promptData, mySettings);
                MyPromptWindow.Show();
            });

            return await MyPromptWindow!.GetUserInput();
        }

        public Task<bool> FalseResult = Task.Run(() =>
            {
                return false;
            });

        public async Task<bool> HandleRequest(object sender, PromptData args)
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
                return await PromptUser(args.defaultPrompt);
        }
    }
}
