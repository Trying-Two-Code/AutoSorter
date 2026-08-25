using System.Windows;
using System.Windows.Controls;

namespace App.Ui.UiComponents;

public partial class StartStopButton : UserControl
{
    private bool _isRunning;

    public bool IsRunning => _isRunning;

    public StartStopButton()
    {
        InitializeComponent();
    }

    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(
            name: "Click",
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(StartStopButton));

    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    private void StartStopButtonInst_Click(
        object sender,
        RoutedEventArgs e)
    {
        _isRunning = !_isRunning;

        StartStopButtonInst.Content =
            _isRunning ? "STOP" : "START";

        var args = new RoutedEventArgs(
            ClickEvent,
            this);

        RaiseEvent(args);
    }
}