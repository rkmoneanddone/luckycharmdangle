using System;
using System.IO;
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
}