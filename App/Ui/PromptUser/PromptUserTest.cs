using Core;
using Core.Sorter.Test;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using static Core.Sorter.Test.TestData;

namespace App.Ui.PromptUser
{
    internal class PromptUserTest
    {
        public TestData? MyTestData { get; set; } = null;

        public async Task<bool> TestPrompt(PromptData promptData, PromptWindow promptWindow)
        {
            CurrentWindow = promptWindow;

            promptWindow.NewInfoWindowMade += OnNewInfoWindowMade; 

            MyTestData = promptData.TestData;

            for (int i = 0; i < MyTestData!.actions.Count; i++)
            {
                TestAction action = MyTestData.actions[i];

                switch (action)
                {
                    case (TestAction.ClickYes):
                        promptWindow.OnYesPressed(this, EventArgs.Empty);
                        break;

                    case (TestAction.ClickNo):
                        promptWindow.OnNoPressed(this, EventArgs.Empty);
                        break;

                    case (TestAction.ClickNewRuleInfo):
                        promptWindow.OnNewRuleInfoClicked(this, null);
                        break;

                    case (TestAction.ClickFileInfo):
                        promptWindow.OnFileInfoClicked(this, null);
                        break;

                    case (TestAction.ClickOldRuleInfo):
                        promptWindow.OnOldRuleInfoClicked(this, null);
                        break;

                    case (TestAction.ClickSettings):
                        promptWindow.OnSettingsClicked(this, null);
                        break;

                    case (TestAction.CloseActiveWindow):
                        CurrentWindow.Close();
                        break;

                    default:
                        throw new NonExistantTestActionException();
                }
            }

            bool decision = GetDecision(promptWindow);

            if (promptWindow.IsLoaded)
            {
                promptWindow.Close();
            }

            return decision;
        }

        private Window? CurrentWindow { get; set; }

        private void OnNewInfoWindowMade(object? sender, Window? newWindow)
        {
            CurrentWindow = newWindow;
        }

        /// <summary>
        /// bypasses user decision since during testing it would not 
        /// be chill to force the dev to manually input buttons
        /// </summary>
        private bool GetDecision(PromptWindow promptWindow)
        {
            bool result = promptWindow.GetUserInput().Result;
            return result;
        }

        internal class NonExistantTestActionException : Exception { }
    }
}
