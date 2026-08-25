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
