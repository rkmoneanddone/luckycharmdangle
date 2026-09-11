using System.Diagnostics;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using LuckyDangle.UI.Support;

namespace LuckyDangle.UI.About;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        VersionText.Text = $"Version {GetInstalledVersion()}";

        string imagePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "Assets",
                "LuckyDangle.png");

        AboutLogo.Source =
            new BitmapImage(
                new Uri(
                    imagePath,
                    UriKind.Absolute));
    }

    private static string GetInstalledVersion()
    {
        try
        {
            var packageVersion = Windows.ApplicationModel.Package.Current.Id.Version;
            return $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
        }
        catch
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return version?.ToString(4) ?? "0.0.0.0";
        }
    }
    private void Close_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }

    private void SupportLuckyCharm_Click(
        object sender,
        RoutedEventArgs e)
    {
        var supportWindow =
            new SupportWindow
            {
                Owner = this,
                WindowStartupLocation =
                    WindowStartupLocation.CenterScreen
            };

        supportWindow.Show();
    }

    private void AboutWindow_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void Hyperlink_RequestNavigate(
        object sender,
        System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore browser launch failures; the About window stays usable.
        }

        e.Handled = true;
    }
}
