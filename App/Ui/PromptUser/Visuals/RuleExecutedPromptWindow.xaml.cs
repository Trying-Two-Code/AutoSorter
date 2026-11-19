using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace App.Ui.PromptUser.Visuals
{
    /// <summary>
    /// Interaction logic for RuleExecutedWindow.xaml
    /// </summary>
    public partial class RuleExecutedPromptWindow : Window
    {
        private PromptWindow MyPromptWindow;

        public RuleExecutedPromptWindow()
        {
            InitializeComponent();
            MyPromptWindow = new PromptWindow();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ok_Click(object sender, RoutedEventArgs e)
        {
            MyPromptWindow.DecisionMade(true);
        }

        private void Undo_Click(object sender, RoutedEventArgs e)
        {
            MyPromptWindow.DecisionMade(false);
        }

        private void OnRuleInfoClicked(object sender, RoutedEventArgs e)
        {

        }
        
        private void OnHistoryClicked(object sender, RoutedEventArgs e)
        {

        }

        private void OnOtherRuleInfoClicked(object sender, RoutedEventArgs e)
        {

        }

        private void OnSettingsClicked(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            MyPromptWindow.DecisionMade(false);
        }

        public void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
