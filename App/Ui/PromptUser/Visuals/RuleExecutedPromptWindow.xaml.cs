using App.Ui.UiScripts;
using Core;
using Core.PromptUser;
using Core.Sorter.Creator;
using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace App.Ui.PromptUser.Visuals
{
    /// <summary>
    /// Interaction logic for RuleExecutedWindow.xaml
    /// </summary>
    public partial class RuleExecutedPromptWindow : Window
    {
        private PromptWindow MyPromptWindow;
        private PromptData MyPromptData;
        private Settings mySettings;

        public RuleExecutedPromptWindow(PromptData _promptData, Settings _settings)
        {
            InitializeComponent();
            MyPromptData = _promptData;
            mySettings = _settings;
            MyPromptWindow = new PromptWindow();
        }

        public async Task<PromptResponse> GetUserInput()
        {
            return await MyPromptWindow.MyTaskResult.Task;
        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            MyPromptWindow.DecisionMade(PromptResponse.ResponseOk);
            Close();
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            MyPromptWindow.DecisionMade(PromptResponse.ResponseUndo);
            Close();
        }

        private void OnRuleInfoClicked(object sender, RoutedEventArgs e)
        {
            List<InfoInterfaceItem> newRuleInformation = new()
            {
                new("")
                {
                    LabelText = $"Rule sends files from ({MyPromptData.StartPath}) to ({MyPromptData.EndPath})\n",
                },
                new("")
                {
                    LabelText = "Parameters: \n" + MyPromptData.ParameterInfos
                }
            };
            MyPromptWindow.CreateInformationInterface("Rule Executed", newRuleInformation);
        }
        
        private void OnHistoryClicked(object sender, RoutedEventArgs e)
        {
            List<InfoInterfaceItem> historyInformation = new()
            {
                new()
                {
                    LabelText = $"All automatic file movements: \n",
                }

            };
            if(MyPromptData.FileMoveHistory != null)
            {
                foreach (FileMoveInfo fileMove in MyPromptData.FileMoveHistory)
                {
                    historyInformation.Add(
                        new()
                        { 
                            LabelText = $"file moved from {fileMove.OldPath} to {fileMove.NewPath}"
                        }
                    );
                }
            }
            else
            {
                historyInformation.Add(new()
                {
                    LabelText = $"Is Empty."
                });
            }
            MyPromptWindow.CreateInformationInterface("File Movement History", historyInformation);
        }

        private void OnOtherRuleInfoClicked(object sender, RoutedEventArgs e)
        {
            List<string> otherRuleData = new List<string>()
            {
                "Other Rules:",

            };
        }

        public void OnHaltNotificationsPressed(object? sender, InterfaceItemClickArgs e)
        {
            if(e.DataElement != null)
            {
                System.Windows.Controls.ComboBox myComboBox = (System.Windows.Controls.ComboBox)e.DataElement;
                System.Windows.Controls.TextBox SelectedItem = (System.Windows.Controls.TextBox)myComboBox.SelectionBoxItem;
                TimeSpan? SelectedItemDataContext = (TimeSpan?)SelectedItem.DataContext;

                if (SelectedItemDataContext != null)
                {
                    mySettings.haltRuleExecutedPromptsUntil = (DateTime)(DateTime.Now + SelectedItemDataContext);
                }
            }
            if(e.MuteUntil != null)
            {
                mySettings.haltRuleExecutedPromptsUntil = (DateTime)e.MuteUntil;
            }
        }

        private void OnSettingsClicked(object sender, RoutedEventArgs e)
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

        private void Window_Closed(object sender, EventArgs e)
        {
            MyPromptWindow.DecisionMade(PromptResponse.ResponseClosed);
        }

        public void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
