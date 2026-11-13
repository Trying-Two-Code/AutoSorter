using App.Ui.PromptUser.Visuals;
using App.Ui.UiScripts;
using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace App.Ui.PromptUser
{
    public class UserPrompter
    {
        private PromptWindow? MyPromptWindow;
        private readonly TimeSpan promptAmmount = new TimeSpan(0, 0, 10);
        private DateTime lastPromptTime = DateTime.MinValue;
        private Settings mySettings;

        public UserPrompter(Settings _settings)
        {
            mySettings = _settings;
        }

        public Task<bool> PromptUser(PromptData promptData)
        {
            lastPromptTime = DateTime.Now;

            TaskCompletionSource<bool> PromptCompletionSource = CreatePrompt(promptData);

            Task<bool> promptUserTask = Task.Run(() =>
            {
                return PromptCompletionSource.Task.Result;
            });

            return promptUserTask;
        }

        public TaskCompletionSource<bool> CreatePrompt(PromptData promptData)
        {
            MyPromptWindow = new PromptWindow("Create Rule?", promptData, mySettings);
            MyPromptWindow.Show();
            return MyPromptWindow.GetUserInput();
        }

        public Task<bool> FalseResult = Task.Run(() =>
            {
                return false;
            });

        public Task<bool> HandleRequest(object sender, PromptData args)
        {
            if (DateTime.Now - lastPromptTime < promptAmmount)
            {
                return FalseResult;
            }

            if (args != null)
                return PromptUser(args);
            else
                return PromptUser(args.defaultPrompt);
        }
    }
}
