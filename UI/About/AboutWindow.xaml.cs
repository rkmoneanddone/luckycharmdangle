using System.Windows;
using System.Windows.Input;
namespace LuckyDangle.UI.About;


public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void Close_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
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

