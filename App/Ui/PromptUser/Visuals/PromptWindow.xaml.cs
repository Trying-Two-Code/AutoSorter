using App.Ui.PromptUser.Visuals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace App.Ui.PromptUser
{
    /// <summary>
    /// Interaction logic for PromptWindow.xaml
    /// </summary>
    public partial class PromptWindow : Window
    {
        public PromptWindow(string MainPrompt, Dictionary<string, string> ExtraInformation, string title)
        {
            InitializeComponent();
            Title = "AutoSorter - " + title;
        }

        private TaskCompletionSource<bool> MyTaskResult = new();
        public TaskCompletionSource<bool> GetUserInput()
        {
            return MyTaskResult;
        }

        public void DecisionMade(bool result)
        {
            if (MyTaskResult != null)
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

        public void OnNewRuleInfoClicked(object sender, RoutedEventArgs args)
        {
            Debug.WriteLine("New Rule!");
        }

        public void OnFileInfoClicked(object sender, RoutedEventArgs args)
        {
            Debug.WriteLine("File Info!");
        }

        public void OnOldRuleInfoClicked(object sender, RoutedEventArgs args)
        {
            Debug.WriteLine("Old Rule!");
        }

        public void OnSettingsClicked(object sender, RoutedEventArgs args)
        {
            Debug.WriteLine("Settings!");
        }

        public void Window_Closed(object sender, EventArgs args)
        {
            DecisionMade(false);
        }

        public void Window_Deactivated(object sender, EventArgs args)
        {
            //wait
        }
    }
}
