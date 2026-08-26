using System;
using System.Windows;
using System.Windows.Media;
using BCore;
using App.Ui.UiComponents;
using Syroot.Windows.IO;
using System.Diagnostics;

namespace AutoSorter;

public partial class MainWindow : Window
{
    private readonly AppAPI _app;

    public MainWindow()
    {
        InitializeComponent();

        string path = DownloadFolder();
        _app = new AppAPI(path);

        Loaded += new RoutedEventHandler(Window_Loaded);
        System.Diagnostics.Debug.WriteLine("started.");
    }
    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Setup_Window_Position();
    }

    private void Setup_Window_Position()
    {

        var desktopWorkingArea = SystemParameters.WorkArea;
        Left = desktopWorkingArea.Right - Width;
        Top = desktopWorkingArea.Bottom - Height;
    }

    private static string DownloadFolder()
    {
        string downloadsPath = KnownFolders.Downloads.Path;
        return downloadsPath;
    }

    private void StartStopButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not StartStopButton button)
            return;

        if (button.IsRunning)
        {
            _app.Start();

            CreateSound(
                @"Ui\Assets\Audio\SFX-Success1.mp3");
        }
        else
        {
            _app.Stop();
        }
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if(sender is not TopbarButton button)
            return;

        SendToTray();
    }

    private void SendToTray()
    {
        ShowInTaskbar = false;
        WindowState = WindowState.Minimized;
    }

    private void CreateSound(string soundFilePath)
    {
        var player = new MediaPlayer();

        player.Open(
            new Uri(
                soundFilePath,
                UriKind.RelativeOrAbsolute));

        player.Play();
    }

    protected override void OnClosed(EventArgs e)
    {
        _app.Stop();

        base.OnClosed(e);
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        BringBackWindow(sender, e, true);
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        BringBackWindow(sender, e, false);
    }

    private void BringBackWindow(object sender, EventArgs e, bool transparent)
    {
        var window = (Window)sender;
        window.Topmost = true;

        if (!window.Activate())
            Debug.WriteLine("Could not bring to foreground.");
        
        window.Background.Opacity = transparent ? 0.1f : 1.0f;
    }
}