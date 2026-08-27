using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Forms;
//https://possemeeg.wordpress.com/2007/09/06/minimize-to-tray-icon-in-wpf/
namespace App.Ui.UiScripts;

internal class TrayIconHandler
{
    private System.Windows.Forms.NotifyIcon m_notifyIcon;

    public TrayIconHandler()
    {
        m_notifyIcon = new System.Windows.Forms.NotifyIcon();
        m_notifyIcon.BalloonTipText = "Autosorter has been minimised. Click the tray icon to show.";
        m_notifyIcon.BalloonTipTitle = "Open Autosorter";
        m_notifyIcon.Text = "Autosorter";
        m_notifyIcon.Icon = GetIcon("Ui/Assets/Visual/logo.ico"); // TODO null check
        m_notifyIcon.Click += new EventHandler(m_notifyIcon_Click);
    }

    static public Icon? GetIcon(string path)
    {
        string fullPath = System.IO.Path.GetFullPath(path);
        Icon? appIcon = Icon.ExtractAssociatedIcon(fullPath);
        return appIcon;
    }

    void OnClose(object sender, CancelEventArgs args)
    {
        m_notifyIcon.Dispose();
        m_notifyIcon = null;
    }

    private WindowState m_storedWindowState = WindowState.Normal;
    void OnStateChanged(object sender, EventArgs args)
    {
        Window window = System.Windows.Application.Current.MainWindow;

        if (window.WindowState == WindowState.Minimized)
        {
            window.Hide();
            if (m_notifyIcon != null)
                m_notifyIcon.ShowBalloonTip(2000);
        }
        else
            m_storedWindowState = window.WindowState;
    }

    void m_notifyIcon_Click(object sender, EventArgs e)
    {
        Window window = System.Windows.Application.Current.MainWindow;

        window.Show();
        window.WindowState = m_storedWindowState;
    }

    public void ShowTrayIcon(bool show)
    {
        if (m_notifyIcon != null)
            m_notifyIcon.Visible = show;
    }

}
