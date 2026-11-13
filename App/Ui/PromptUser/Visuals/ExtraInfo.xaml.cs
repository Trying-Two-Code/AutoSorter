using App.Ui.UiComponents;
using System.Windows;
using System.Windows.Controls;

namespace App.Ui.PromptUser.Visuals
{
    /// <summary>
    /// Interaction logic for ExtraInfo.xaml
    /// </summary>
    public partial class ExtraInfo : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty InfoLabelProperty =
            DependencyProperty.Register(
                "InfoLabel",
                typeof(string),
                typeof(ExtraInfo),
                new PropertyMetadata(
                    OnInfoLabelChanged)
                );

        public static void OnInfoLabelChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
            )
        {
            ExtraInfo extraInfo = (ExtraInfo)d;

            extraInfo.LabelTextBlock.Text = (string)e.NewValue;
        }

        public string InfoLabel
        {
            get => (string)GetValue(InfoLabelProperty);
            set => SetValue(InfoLabelProperty, value);
        }

        public static readonly DependencyProperty ClickProperty =
            DependencyProperty.Register(
                "Click",
                typeof(RoutedEventHandler),
                typeof(ExtraInfo),
                new PropertyMetadata(
                    OnClickChanged)
                );

        public static void OnClickChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e
            )
        {
            ExtraInfo extraInfo = (ExtraInfo)d;

            extraInfo.MainButton.Click += (RoutedEventHandler)e.NewValue;
        }

        public RoutedEventHandler Click
        {
            get => (RoutedEventHandler)GetValue(ClickProperty);
            set => SetValue(ClickProperty, value);
        }

        public ExtraInfo()
        {
            InitializeComponent();
        }
    }
}
