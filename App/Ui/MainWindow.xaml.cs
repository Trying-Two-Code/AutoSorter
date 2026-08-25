using System;
using System.Windows;
using System.Windows.Media;
using BCore;
using App.Ui.UiComponents;
using Syroot.Windows.IO;

namespace AutoSorter;

public partial class MainWindow : Window
{
    private readonly AppAPI _app;

    public MainWindow()
    {
        InitializeComponent();

        string path = DownloadFolder();

        _app = new AppAPI(path);
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

        Shutdown();
    }

    private void Shutdown()
    {
        DEBUGGGERRR.Text = "working";
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
}