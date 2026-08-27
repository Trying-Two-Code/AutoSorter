using System;
using System.Windows;
using System.Windows.Media;
using BCore;
using App.Ui.UiComponents;
using Syroot.Windows.IO;
using System.Diagnostics;
using System.Timers;

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

        CreateSound(
            @"Ui\Assets\Audio\SFX-Click2.mp3");

        if (button.IsRunning)
        {
            _app.Start();
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
        WindowState = WindowState.Minimized;
        ShowInTaskbar = false;
        throw new NotImplementedException();
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

    private void BringBackWindow(object sender, EventArgs e, bool translucent = false)
    {
        var window = (Window)sender;
        window.Topmost = true;

        if (!window.Activate())
            Debug.WriteLine("Could not bring to foreground.");

        dealWithWindowOpacity(window, translucent);
    }

    CancellationTokenSource cts;

    private void dealWithWindowOpacity(Window window, bool translucent)
    {
        const float highestOpacity = 1f;
        if (translucent)
        {
            cts = new CancellationTokenSource();
            Fadeout(window, cts.Token);
        }
        else if (window.Opacity <= highestOpacity)
        {
            if(cts != null)
                cts.Cancel();
            window.Opacity = highestOpacity;
        }
        else
        {
            return;
        }
    }

    async private void Fadeout(Window window, CancellationToken fadeCancel)
    {
        while(window.Opacity > 0f)
        {
            if (fadeCancel.IsCancellationRequested)
            {
                return;
            }
            window.Opacity -= .01f;
            await Task.Delay(500);
        }

        return;
    }
}