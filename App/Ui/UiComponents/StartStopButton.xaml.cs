using Helper.DataGathering;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace App.Ui.UiComponents;

public partial class StartStopButton : System.Windows.Controls.UserControl
{
    //:::TODO::: isrunning variable should persist app close
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
        ToggleRunning();

        var args = new RoutedEventArgs(
            ClickEvent,
            this);

        RaiseEvent(args);
    }

    public void ToggleRunning()
    {
        _isRunning = !_isRunning;

        const string StartSource = @"\Ui\Assets\Visual\record.png";
        const string PauseSource = @"\Ui\Assets\Visual\stop.png";

        StartStopButtonInstImage.Source =
            _isRunning ? GetSource(PauseSource) : GetSource(StartSource);
    }

    private System.Windows.Media.ImageSource GetSource(string path, int width = 200, int height = 200)
    {
        BitmapImage image = new BitmapImage();
        image.BeginInit();
        Uri _uri = new Uri(path, UriKind.RelativeOrAbsolute);
        image.UriSource = _uri;
        image.EndInit();

        return image;
    }
}