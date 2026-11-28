using App.Ui.PromptUser.Visuals;
using App.Ui.UiScripts;
using Core;
using Core.PromptUser;
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

        public async Task<PromptResponse> GetUserInput()
        {
            return await MyPromptWindow.MyTaskResult.Task;
        }

        public void OnYesPressed(object sender, EventArgs args)
        {
            //send true
            MyPromptWindow.DecisionMade(PromptResponse.ResponseOk);
            Close();
        }

        public void OnNoPressed(object sender, EventArgs args)
        {
            //send false
            MyPromptWindow.DecisionMade(PromptResponse.ResponseNo);
            Close();
        }

        public void OnHaltNotificationsPressed(object? sender, InterfaceItemClickArgs e)
        {
            Debug.WriteLine("RECIEVED");
            if (e.DataElement != null)
            {
                try
                {
                    System.Windows.Controls.ComboBox myComboBox = (System.Windows.Controls.ComboBox)e.DataElement;
                    Debug.WriteLine(myComboBox.SelectedValue);
                    System.Windows.Controls.ComboBoxItem SelectedItem = (System.Windows.Controls.ComboBoxItem)myComboBox.SelectedItem;
                    TimeSpan? SelectedItemDataContext = (TimeSpan?)SelectedItem.Tag;

                    Debug.WriteLine(SelectedItemDataContext.ToString());

                    if (SelectedItemDataContext != null)
                    {
                        mySettings.haltRuleMadePromptsUntil = (DateTime)(DateTime.Now + SelectedItemDataContext);
                    }
                } 
                catch(System.InvalidCastException InvalidCastEx)
                {
                    Debug.WriteLine(InvalidCastEx);
                }
            }
            if (e.MuteUntil != null)
            {
                mySettings.haltRuleExecutedPromptsUntil = (DateTime)e.MuteUntil;
            }
        }


        public void OnNewRuleInfoClicked(object sender, RoutedEventArgs args)
        {
            List<InfoInterfaceItem> newRuleInformation = new()
            {
                new()
                {
                    LabelText = $"Rule sends files from ({myPromptInformation.StartPath}) to ({myPromptInformation.EndPath})\n"
                },
                new()
                {
                    LabelText = "Parameters: \n" + myPromptInformation.ParameterInfos
                }
            };
            MyPromptWindow.CreateInformationInterface("New Rule", newRuleInformation);
        }

        public void OnFileInfoClicked(object sender, RoutedEventArgs args)
        {
            List<InfoInterfaceItem> fileInfos = new();
            foreach(string fileInfo in myPromptInformation.FileMoveInfos)
            {
                fileInfos.Add(
                    new() { LabelText = fileInfo });
            }
            MyPromptWindow.CreateInformationInterface("Old File Info", fileInfos);
        }

        public void OnOldRuleInfoClicked(object sender, RoutedEventArgs args)
        {
            List<InfoInterfaceItem> OldRuleInfo = myPromptInformation
                .OldRulesData
                .Select(stringdata =>
                {
                    return new InfoInterfaceItem() { LabelText = stringdata };
                }).ToList();
            MyPromptWindow.CreateInformationInterface("Old Rules", OldRuleInfo);
        }

        public void OnSettingsClicked(object sender, RoutedEventArgs args)
        {
            List<InfoInterfaceItem> SettingsInterface = new()
            {
                new(){
                    LabelText = "sound muted: " + mySettings.muted.ToString(),
                },
                new(){
                    LabelText = "prompt ammount: " + mySettings.promptUserAmm.ToString(),
                },
                new(){
                    LabelText = "running: " + mySettings.running.ToString()
                },
                new("MutePrompts")
                {
                    LabelText = "prompts muted: " + (mySettings.haltRuleExecutedPromptsUntil > DateTime.Now),
                    ClickHandler = new EventHandler<InterfaceItemClickArgs>(OnHaltNotificationsPressed),
                    ChildContent = InfoInterfaceUserControls.MuteComboBox
                }
            };
            MyPromptWindow.CreateInformationInterface("Settings", SettingsInterface);
        }

        public void Window_Closed(object sender, EventArgs args)
        {
            MyPromptWindow.DecisionMade(PromptResponse.ResponseClosed);
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
