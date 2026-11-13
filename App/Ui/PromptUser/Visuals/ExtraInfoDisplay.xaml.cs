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
    /// Interaction logic for ExtraInfoDisplay.xaml
    /// </summary>
    public partial class ExtraInfoDisplay : Window
    {
        private List<TextBlock> MyTextBlocks = new();

        public ExtraInfoDisplay(string _title, List<string> _information)
        {
            InitializeComponent();
            Title = _title;
            CreateTexts(_information);
        }

        public void CreateTexts(List<string> texts)
        {
            foreach(string text in texts)
            {
                CreateTextBlock(text);
            }
        }

        public TextBlock CreateTextBlock(string text)
        {
            TextBlock newTextBlock = new();
            newTextBlock.Text = text;
            MyTextBlocks.Add(newTextBlock);

            AllInfo.Children.Add(newTextBlock);

            return newTextBlock;
        }

        public EventHandler<ExtraInfoDisplay, EventArgs>? OnCloseDisplay;

        private void Window_Closed(object sender, EventArgs e)
        {
            OnCloseDisplay?.Invoke(
                (ExtraInfoDisplay)this, 
                EventArgs.Empty);
        }
    }
}
