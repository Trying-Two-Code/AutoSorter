using App.Ui.PromptUser.Visuals;
using Core.PromptUser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace App.Ui.PromptUser
{
    class PromptWindow
    {
        public EventHandler<Window> NewInfoInterfaceMade { get; set; }

        private List<UserInfoInterface> OpenExtraInfoDisplays = new();
        private List<string> OpenExtraInfoDisplayNames = new();
        public void CreateInformationInterface(string title, List<InfoInterfaceItem> data)
        {
            UserInfoInterface NewInfoInterface = new UserInfoInterface(title, data);
            bool AlreadyOpened = OpenExtraInfoDisplayNames.Contains(title);

            if (AlreadyOpened)
            {
                NewInfoInterface.Close();
                UserInfoInterface MatchingDisplay = OpenExtraInfoDisplays.Where(display => display.Title == title).First();
                MatchingDisplay.Focus();
            }
            else
            {
                NewInfoInterface.Show();
                NewInfoInterface.OnCloseDisplay += OnCloseInformationWindow;
                OpenExtraInfoDisplays.Add(NewInfoInterface);
                OpenExtraInfoDisplayNames.Add(title);
            }

            NewInfoInterfaceMade?.Invoke(this, NewInfoInterface);
        }
        public void OnCloseInformationWindow(UserInfoInterface extraInfoDisplay, EventArgs args)
        {
            extraInfoDisplay.OnCloseDisplay -= OnCloseInformationWindow;
            OpenExtraInfoDisplays.Remove(extraInfoDisplay);
            OpenExtraInfoDisplayNames.Remove(extraInfoDisplay.Title);
        }

        public TaskCompletionSource<PromptResponse> MyTaskResult { get; private set; } = new();

        public void DecisionMade(PromptResponse result)
        {
            if (MyTaskResult != null && !MyTaskResult.Task.IsCompleted)
            {
                MyTaskResult.SetResult(result);
            }
        }
    }
}
