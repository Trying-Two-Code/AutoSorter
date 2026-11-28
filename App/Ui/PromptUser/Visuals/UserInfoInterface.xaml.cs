using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace App.Ui.PromptUser.Visuals
{
    /// <summary>
    /// Interaction logic for ExtraInfoDisplay.xaml
    /// </summary>
    public partial class UserInfoInterface : Window
    {
        private List<TextBlock> MyTextBlocks = new();

        public UserInfoInterface(string _title, List<InfoInterfaceItem> _information)
        {
            InitializeComponent();
            Title = _title;
            CreateInfoItems(_information);
        }

        public void CreateInfoItems(List<InfoInterfaceItem> _items)
        {
            foreach(InfoInterfaceItem InfoInterfaceItem in _items)
            {
                CreateInfoItem(InfoInterfaceItem);
            }
        }

        public TextBlock CreateInfoItem(InfoInterfaceItem item)
        {
            StackPanel Parent = new();
            AllInfo.Items.Add(Parent);

            TextBlock newTextBlock = new();
            Parent.Children.Add(newTextBlock);
            if(item?.ChildContent != null)
            {
                Parent.Children.Add(item.ChildContent);
            }

            newTextBlock.Name = item.Name;
            newTextBlock.Text = item.LabelText;
            MyTextBlocks.Add(newTextBlock);

            StyleTextBlock(newTextBlock);


            if (item?.ClickHandler != null)
            {
                newTextBlock.MouseLeftButtonDown += (s, e) =>
                {
                    Debug.WriteLine("PRESSED -- not working...");
                    item.ClickHandler?.Invoke(this, new() { DataElement = item?.ChildContent });
                };
            }

            return newTextBlock;
        }

        private void StyleTextBlock(TextBlock textBlock)
        {
            textBlock.FontFamily = new System.Windows.Media.FontFamily("Bahnschrift");
            textBlock.Foreground = System.Windows.Media.Brushes.White;
            textBlock.FontSize = 20;
            textBlock.Padding = new Thickness(10, 10, 10, 10);
        }

        public EventHandler<UserInfoInterface, EventArgs>? OnCloseDisplay;

        private void Window_Closed(object sender, EventArgs e)
        {
            OnCloseDisplay?.Invoke(
                (UserInfoInterface)this, 
                EventArgs.Empty);
        }
    }
}
