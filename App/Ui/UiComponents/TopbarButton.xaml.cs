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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace App.Ui.UiComponents
{
    /// <summary>
    /// Interaction logic for TopbarButton.xaml
    /// </summary>
    public partial class TopbarButton : UserControl
    {
        public TopbarButton()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(
            "Source",
            typeof(string),
            typeof(TopbarButton),
            new PropertyMetadata(
                OnSourceChanged)
            );

        public static void OnSourceChanged(
            DependencyObject d, 
            DependencyPropertyChangedEventArgs e
            )
        {
            TopbarButton content = (TopbarButton)d;

            content.StartStopButtonInstImage.Source = content.getImageFromPath((string)e.NewValue);
        }

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private ImageSource getImageFromPath(string path)
        {
            // Copied from microsoft docs
            // Create source.
            BitmapImage bi = new BitmapImage();
            // BitmapImage.UriSource must be in a BeginInit/EndInit block.
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bi.EndInit();

            return bi;
        }

        public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(
            name: "Click",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(TopbarButton));

        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        private void TopbarButtonInst_Click(
            object sender,
            RoutedEventArgs e)
        {
            var args = new RoutedEventArgs(
                ClickEvent,
                this);

            RaiseEvent(args);
        }
    }
}
