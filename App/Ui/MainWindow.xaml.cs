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



namespace AutoSorter;

public partial class MainWindow : Window
{
    private readonly AppAPI _app;
    public MainWindow()
    {

        InitializeComponent();

        string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _app = new AppAPI(path, DownloadFolder());
        

        Loaded += new RoutedEventHandler(Window_Loaded);

        UserSettings.GetSettings();
        if (UserSettings.running)
        {
            System.Diagnostics.Debug.WriteLine("starting.");
            _app.Start();
            MainStartStopButton.ToggleRunning();
        }
        System.Diagnostics.Debug.WriteLine(UserSettings.running);

        RunOnStartup();
    }

    void RunOnStartup()
    {
        //Send to registry
        string RegistryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        RegistryKey regKey = Registry.CurrentUser.OpenSubKey(RegistryPath, true);
        Debug.WriteLine("going to regKey: " + regKey);

        string ExecutionPath = System.Windows.Forms.Application.ExecutablePath.ToString();
        //ExecutionPath = @"C:\Users\Drago\source\repos\AutoSorter\bin\App\Debug\net10.0-windows\App.exe";

        if (regKey.GetValue("AutoSorter") != ExecutionPath)
        {
            regKey.SetValue("AutoSorter", System.Windows.Forms.Application.ExecutablePath.ToString());
            Debug.WriteLine("set regKey value: " + regKey.GetValue("AutoSorter"));
        }
        
        //as a backup, make a shortcut in startup folder
        string StartupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        CreateShortcut(StartupFolder, ExecutionPath);
    }

    private void CreateShortcut(string shortCutPath, string shortCutReferences)
    {
        WshShell shell = new WshShell();
        string shortcutAddress = Path.Combine(shortCutPath, @"AutoSorter.lnk");
        IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
        shortcut.Description = "Shortcut to AutoSorter";
        //shortcut.Hotkey = "Ctrl+Shift+A";
        shortcut.TargetPath = shortCutReferences;
        shortcut.WorkingDirectory = Path.GetDirectoryName(shortCutReferences);
        shortcut.Save();
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
            _app.Start();
        }
        else
        {
            UserSettings.Set<bool>("running", false);
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
        string log = "data/log.txt";
        string userAction = "data/userAction.json";
        string showUsing = "notepad.exe";

        Process.Start(showUsing, log);
        Process.Start(showUsing, userAction);
    }
}