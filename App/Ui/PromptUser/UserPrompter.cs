using App.Ui.PromptUser.Visuals;
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

        public TaskCompletionSource<bool> CreatePrompt(PromptData prompt)
        {
            MyPromptWindow = new PromptWindow(prompt.PromptString, null, "Create Rule?");
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

            string? promptString = args.PromptString;
            string? promptParamaters = args.ParameterInfos;
            string? promptFileMovements = args.FileMoveInfos;

            if (promptString != null)
                return PromptUser(args);
            else
                return PromptUser(args.defaultPrompt);
        }
    }
}
