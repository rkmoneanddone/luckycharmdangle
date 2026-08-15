using System.Windows;
using System.Windows.Input;
using LuckyDangle.UI.Support;

namespace LuckyDangle.UI.About;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }


    // =====================================================
    // CLOSE
    // =====================================================

    private void Close_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }


    // =====================================================
    // SUPPORT LUCKYCHARM
    // =====================================================

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

        supportWindow.ShowDialog();
    }


    // =====================================================
    // DRAG WINDOW
    // =====================================================

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