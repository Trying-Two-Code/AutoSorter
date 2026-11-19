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
    public partial class RuleMadePromptWindow : Window
    {
        private PromptWindow MyPromptWindow;
        public EventHandler<Window> NewInfoWindowMade { get; set; }
        private PromptData myPromptInformation;
        private Settings mySettings;
        public RuleMadePromptWindow(string title, PromptData _promptInformation, Settings _settings)
        {
            InitializeComponent();
            myPromptInformation = _promptInformation;
            mySettings = _settings;
            MyPromptWindow = new PromptWindow();
        }

        public async Task<bool> GetUserInput()
        {
            return await MyPromptWindow.MyTaskResult.Task;
        }

        public void OnYesPressed(object sender, EventArgs args)
        {
            //send true
            MyPromptWindow.DecisionMade(true);
        }

        public void OnNoPressed(object sender, EventArgs args)
        {
            //send false
            MyPromptWindow.DecisionMade(false);
        }

        public void OnHaltNotificationsPressed()
        {

        }


        public void OnNewRuleInfoClicked(object sender, RoutedEventArgs args)
        {
            List<string> newRuleInformation = new()
            {
                $"Rule sends files from ({myPromptInformation.StartPath}) to ({myPromptInformation.EndPath})\n",
                "Parameters: \n" + myPromptInformation.ParameterInfos
            };
            MyPromptWindow.CreateInformationWindow("New Rule", newRuleInformation);
        }

        public void OnFileInfoClicked(object sender, RoutedEventArgs args)
        {
            List<string> fileInfos = new();
            foreach(string fileInfo in myPromptInformation.FileMoveInfos)
            {
                fileInfos.Add(fileInfo);
            }
            MyPromptWindow.CreateInformationWindow("Old File Info", fileInfos);
        }

        public void OnOldRuleInfoClicked(object sender, RoutedEventArgs args)
        {
            List<string> OldRuleInfo = myPromptInformation.OldRulesData;
            MyPromptWindow.CreateInformationWindow("Old Rules", OldRuleInfo);
        }

        public void OnSettingsClicked(object sender, RoutedEventArgs args)
        {
            List<string> settingsString = new(){
                "sound muted: " + mySettings.muted.ToString(),
                "prompt ammount: " + mySettings.promptUserAmm.ToString(),
                "running: " + mySettings.running.ToString()
                };
            MyPromptWindow.CreateInformationWindow("Settings", settingsString);
        }

        public void Window_Closed(object sender, EventArgs args)
        {
            MyPromptWindow.DecisionMade(false);
        }

        public void Window_Deactivated(object sender, EventArgs args)
        {
            //wait
        }
        public void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
