using App.Ui.UiComponents;
using App.Ui.UiScripts;
using BCore;
using Helper.DataGathering;
using Syroot.Windows.IO;
using System;
using System.Diagnostics;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Microsoft.Win32;
using IWshRuntimeLibrary;
using System.Windows.Threading;
using Core.Sorter;
using Core;



namespace AutoSorter;

public partial class MainWindow : Window
{
    private readonly AppAPI _app_Download;
    private readonly AppAPI _app_Desktop;
    private readonly AppAPI _app_Documents;
    public MainWindow()
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        InitializeComponent();

        string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _app_Download = new AppAPI(path,  KnownFolders.Downloads.Path, OnUIEventRecieved, OnGetSettingsRecieved);
        _app_Desktop = new AppAPI(path,   KnownFolders.Desktop.Path,   OnUIEventRecieved, OnGetSettingsRecieved);
        _app_Documents = new AppAPI(path, KnownFolders.Documents.Path, OnUIEventRecieved, OnGetSettingsRecieved);

        Loaded += new RoutedEventHandler(Window_Loaded);

        UserSettings.GetSettings();
        if (UserSettings.running)
        {
            MainStartStopButton.ToggleRunning();
        }

        ActivateOnStartup.RegisterApp();
        if(ActivateOnStartup.DetectClose())
            SendToTray();
    }

    private void StartApps()
    {
        _app_Download.Start();
        _app_Desktop.Start();
        _app_Documents.Start();
    }
    private void StopApps()
    {
        _app_Download.Stop();
        _app_Desktop.Stop();
        _app_Documents.Stop();
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
            UserSettings.Set<bool>("running", true);
            StartApps();
        }
        else
        {
            UserSettings.Set<bool>("running", false);
            StopApps();
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

    private TrayIconHandler TrayHandler = new TrayIconHandler();
    private void SendToTray()
    {
        WindowState = WindowState.Minimized;
        ShowInTaskbar = false;
        TrayHandler.ShowTrayIcon(true);
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
        StopApps();

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

    private void ShowUserdata_Click(object sender, RoutedEventArgs e)
    {
        string log = Path.Combine(AppContext.BaseDirectory, "data/log.txt");
        string userAction = Path.Combine(AppContext.BaseDirectory, "data/userAction.json");
        string showUsing = "notepad.exe";

        Process.Start(showUsing, log);
        Process.Start(showUsing, userAction);
    }

    //public Dispatcher PromptUserDispatcher;

    public Task<bool> PromptUser(string prompt)
    {
        Task<bool> promptUserTask = Task.Run(() =>
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(prompt, "should AutoSorter make a rule?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
            return result == MessageBoxResult.Yes;
        });

        return promptUserTask;
    }

    public Task<bool> OnUIEventRecieved(object sender, EventArgs args)
    {
        string? promptString    = (string?)args?.GetType()?.GetProperty("PromptString")?.GetValue(args);
        string? promptExtra     = (string?)args?.GetType()?.GetProperty("ExtraInfo")?.GetValue(args);

        if (promptString != null)
            return PromptUser(promptString);
        else
            return PromptUser("Create a rule? No info provided.");
    }

    public settings GetSettings()
    {
        UserSettings.GetSettings();

        settings _settings = new()
        {
            running = UserSettings.running,
            promptUserAmm = UserSettings.promptUserAmm,
            muted = UserSettings.muted
        };

        return _settings;
    }

    public object OnGetSettingsRecieved(object sender, EventArgs args)
    {
        return GetSettings();
    }
}