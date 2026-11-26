using Core;
using Core.PromptUser;
using Core.Sorter.Creator;
using System;
using System.Collections.Generic;
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

        public RuleExecutedPromptWindow(PromptData _promptData)
        {
            InitializeComponent();
            MyPromptData = _promptData;
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
            List<string> newRuleInformation = new()
            {
                $"Rule sends files from ({MyPromptData.StartPath}) to ({MyPromptData.EndPath})\n",
                "Parameters: \n" + MyPromptData.ParameterInfos
            };
            MyPromptWindow.CreateInformationWindow("Rule Executed", newRuleInformation);
        }
        
        private void OnHistoryClicked(object sender, RoutedEventArgs e)
        {
            List<string> historyInformation = new()
            {
                $"All automatic file movements: \n",

            };
            if(MyPromptData.FileMoveHistory != null)
            {
                foreach (FileMoveInfo fileMove in MyPromptData.FileMoveHistory)
                {
                    historyInformation.Add(
                        $"file moved from {fileMove.OldPath} to {fileMove.NewPath}"
                    );
                }
            }
            else
            {
                historyInformation.Add("Is empty (:");
            }
            MyPromptWindow.CreateInformationWindow("File Movement History", historyInformation);
        }

        private void OnOtherRuleInfoClicked(object sender, RoutedEventArgs e)
        {
            List<string> otherRuleData = new List<string>()
            {
                "Other Rules:",

            };
        }

        private void OnSettingsClicked(object sender, RoutedEventArgs e)
        {

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
