using App.Ui.PromptUser.Visuals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace App.Ui.PromptUser
{
    class PromptWindow
    {
        public EventHandler<Window> NewInfoWindowMade { get; set; }

        private List<ExtraInfoDisplay> OpenExtraInfoDisplays = new();
        private List<string> OpenExtraInfoDisplayNames = new();
        public void CreateInformationWindow(string title, List<string> data)
        {
            ExtraInfoDisplay newInfoDisplay = new ExtraInfoDisplay(title, data);
            bool AlreadyOpened = OpenExtraInfoDisplayNames.Contains(title);

            if (AlreadyOpened)
            {
                newInfoDisplay.Close();
                ExtraInfoDisplay MatchingDisplay = OpenExtraInfoDisplays.Where(display => display.Title == title).First();
                MatchingDisplay.Focus();
            }
            else
            {
                newInfoDisplay.Show();
                newInfoDisplay.OnCloseDisplay += OnCloseInformationWindow;
                OpenExtraInfoDisplays.Add(newInfoDisplay);
                OpenExtraInfoDisplayNames.Add(title);
            }

            NewInfoWindowMade?.Invoke(this, newInfoDisplay);
        }
        public void OnCloseInformationWindow(ExtraInfoDisplay extraInfoDisplay, EventArgs args)
        {
            extraInfoDisplay.OnCloseDisplay -= OnCloseInformationWindow;
            OpenExtraInfoDisplays.Remove(extraInfoDisplay);
            OpenExtraInfoDisplayNames.Remove(extraInfoDisplay.Title);
        }

        public TaskCompletionSource<bool> MyTaskResult { get; private set; } = new();

        public void DecisionMade(bool result)
        {
            if (MyTaskResult != null && !MyTaskResult.Task.IsCompleted)
            {
                MyTaskResult.SetResult(result);
            }
        }
    }
}
