using App.Ui.UiComponents;
using App.Ui.UiScripts;
using BCore;
using Syroot.Windows.IO;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using System.Windows;
using System.Windows.Media;

namespace AutoSorter;

public partial class MainWindow : Window
{
    private readonly AppAPI _app_Download;
    private readonly AppAPI _app_Desktop;
    private readonly AppAPI _app_Documents;

    private readonly HandleWindow _handleWindow;
    private readonly UserPrompter _userPrompter;

    private Settings _settings;

    public MainWindow()
    {
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        InitializeComponent();

        string path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _app_Download = new AppAPI( path, KnownFolders.Downloads.Path, OnUIEventRecieved, OnGetSettingsRecieved);
        _app_Desktop = new AppAPI(  path, KnownFolders.Desktop.Path,   OnUIEventRecieved, OnGetSettingsRecieved);
        _app_Documents = new AppAPI(path, KnownFolders.Documents.Path, OnUIEventRecieved, OnGetSettingsRecieved);

        Loaded += new RoutedEventHandler(Window_Loaded);

        _settings = UserSettings.GetSettings();
        if (_settings.running)
        {
            MainStartStopButton.ToggleRunning();
        }

        ActivateOnStartup.RegisterApp();
        if(ActivateOnStartup.DetectClose())
            SendToTray();

        _handleWindow = new();
        _userPrompter = new();

        StartApps();
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

    private void StartStopButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not StartStopButton button)
            return;

        StartStop(button.IsRunning);
    }

    private void StartStop(bool start)
    {
        MakeSound sound = new MakeSound(
            @"Ui\Assets\Audio\SFX-Click2.mp3");

        if (start)
        {
            UserSettings.Set("running", true);
            StartApps();
        }
        else if(!start)
        {
            UserSettings.Set("running", false);
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

    protected override void OnClosed(EventArgs e)
    {
        StopApps();

        base.OnClosed(e);
    }

    private void Window_Deactivated(object sender, EventArgs e)
    {
        _handleWindow.BringBackWindow(sender, e, true);
    }

    private void Window_Activated(object sender, EventArgs e)
    {
        _handleWindow.BringBackWindow(sender, e, false);
    }
    
    private void ShowUserdata_Click(object sender, RoutedEventArgs e)
    {
        UserData.Show();
    }

    public Task<bool> OnUIEventRecieved(object sender, EventArgs args)
    {
        return _userPrompter.HandleRequest(sender, args);
    }

    public object OnGetSettingsRecieved(object sender, EventArgs args)
    {
        return UserSettings.GetSettings();
    }
}

class UserPrompter
{
    private readonly TimeSpan promptAmmount = new TimeSpan(0, 0, 10);
    private DateTime lastPromptTime = DateTime.MinValue;

    public Task<bool> PromptUser(string prompt)
    {
        lastPromptTime = DateTime.Now;

        Task<bool> promptUserTask = Task.Run(() =>
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(prompt, "should AutoSorter make a rule?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
            return result == MessageBoxResult.Yes;
        });

        return promptUserTask;
    }

    public Task<bool> FalseResult()
    {
        Task<bool> falsey = Task.Run(() =>
        {
            return false;
        });
        return falsey;
    }

    public Task<bool> HandleRequest(object sender, EventArgs args)
    {
        Debug.WriteLine("Recieved UI event...");
        Debug.WriteLine(lastPromptTime - DateTime.Now);
        //if last prompt time was 10 mins ago, and I can prompt every minute, it should be false
        //the duration between last prompt time and now is less than prompt ammount, it hasn't been long enough
        //lastprompt time = 0:10 now = 0:30 promptAmmount = 0:10; then 0:30 - 0:10 > 0:10
        //lastprompt time = 0:20 now = 0:25 promptAmmount = 0:10; then 0:25 - 0:20 < 0:10
        if (DateTime.Now - lastPromptTime < promptAmmount)
        {
            return FalseResult();
        }

        string? promptString = (string?)args?.GetType()?.GetProperty("PromptString")?.GetValue(args);
        string? promptExtra = (string?)args?.GetType()?.GetProperty("ExtraInfo")?.GetValue(args);

        if (promptString != null)
            return PromptUser(promptString);
        else
            return PromptUser("Create a rule? No info provided.");
    }
}

class HandleWindow
{
    public void BringBackWindow(object sender, EventArgs e, bool translucent = false)
    {
        var window = (Window)sender;
        window.Topmost = true;

        DealWithWindowOpacity(window, translucent);
    }

    CancellationTokenSource? cts;

    private void DealWithWindowOpacity(Window window, bool translucent)
    {
        const float highestOpacity = 1f;
        if (translucent)
        {
            cts = new CancellationTokenSource();
            Fadeout(window, cts.Token);
        }
        else if (window.Opacity <= highestOpacity)
        {
            if (cts != null)
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
        while (window.Opacity > 0f)
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

static class UserData 
{ 
    public static void Show()
    {
        string log = Path.Combine(AppContext.BaseDirectory, "data/log.txt");
        string userAction = Path.Combine(AppContext.BaseDirectory, "data/userAction.json");
        string showUsing = "notepad.exe";

        Process.Start(showUsing, log);
        Process.Start(showUsing, userAction);
    }
}

class MakeSound 
{
    public MakeSound(string soundFilePath)
    {
        MediaPlayer player = new MediaPlayer();

        player.Open(
            new Uri(
                soundFilePath,
                UriKind.RelativeOrAbsolute));

        player.Play();
    }
}