using App.Ui.PromptUser.Visuals;
using App.Ui.UiScripts;
using Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace App.Ui.PromptUser
{
    /// <summary>
    /// Interaction logic for PromptWindow.xaml
    /// </summary>
    public partial class PromptWindow : Window
    {
        public EventHandler<Window> NewInfoWindowMade { get; set; }
        private PromptData myPromptInformation;
        private Settings mySettings;
        public PromptWindow(string title, PromptData _promptInformation, Settings _settings)
        {
            InitializeComponent();
            Title = "AutoSorter - " + title;
            myPromptInformation = _promptInformation;
            mySettings = _settings;
        }

        private TaskCompletionSource<bool> MyTaskResult = new();
        public TaskCompletionSource<bool> GetUserInput()
        {
            return MyTaskResult;
        }

        public void DecisionMade(bool result)
        {
            if (MyTaskResult != null && !MyTaskResult.Task.IsCompleted)
            {
                MyTaskResult.SetResult(result);
            }
            Close();
        }

        public void OnYesPressed(object sender, EventArgs args)
        {
            //send true
            DecisionMade(true);
        }

        public void OnNoPressed(object sender, EventArgs args)
        {
            //send false
            DecisionMade(false);
        }

        public void OnHaltNotificationsPressed()
        {

        }

        private List<ExtraInfoDisplay> OpenExtraInfoDisplays = new();
        private List<string> OpenExtraInfoDisplayNames = new();
        public void CreateInformationWindow(string title, List<string> data)
        {
            ExtraInfoDisplay newInfoDisplay = new ExtraInfoDisplay(title, data);

            if (OpenExtraInfoDisplayNames.Contains(title))
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

        public void OnNewRuleInfoClicked(object sender, RoutedEventArgs args)
        {
            List<string> newRuleInformation = new()
            {
                "Parameters: " + myPromptInformation.ParameterInfos
            };
            CreateInformationWindow("New Rule", newRuleInformation);
        }

        public void OnFileInfoClicked(object sender, RoutedEventArgs args)
        {
            List<string> fileInfos = new();
            foreach(string fileInfo in myPromptInformation.FileMoveInfos)
            {
                fileInfos.Add(fileInfo);
            }
            CreateInformationWindow("Old File Info", fileInfos);
        }

        public void OnOldRuleInfoClicked(object sender, RoutedEventArgs args)
        {
            List<string> OldRuleInfo = myPromptInformation.OldRulesData;
            CreateInformationWindow("Old Rules", OldRuleInfo);
        }

        public void OnSettingsClicked(object sender, RoutedEventArgs args)
        {
            List<string> settingsString = new(){
                "sound muted: " + mySettings.muted.ToString(),
                "prompt ammount: " + mySettings.promptUserAmm.ToString(),
                "running: " + mySettings.running.ToString()
                };
            CreateInformationWindow("Settings", settingsString);
        }

        public void Window_Closed(object sender, EventArgs args)
        {
            DecisionMade(false);
        }

        public void Window_Deactivated(object sender, EventArgs args)
        {
            //wait
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
