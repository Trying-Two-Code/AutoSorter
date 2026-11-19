using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

            AllInfo.Items.Add(newTextBlock);
            StyleTextBlock(newTextBlock);

            return newTextBlock;
        }

        private void StyleTextBlock(TextBlock textBlock)
        {
            textBlock.FontFamily = new System.Windows.Media.FontFamily("Bahnschrift");
            textBlock.Foreground = System.Windows.Media.Brushes.White;
            textBlock.FontSize = 20;
            textBlock.Padding = new Thickness(10, 10, 10, 10);
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
