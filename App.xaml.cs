using System;
using System.Windows;

namespace LuckyDangle;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException +=
            App_DispatcherUnhandledException;

        try
        {
            var window = new MainWindow();

            MainWindow = window;

            window.Show();
        }
        catch (Exception ex)
        {
            ShowFatalError(ex);
        }
    }


    private void App_DispatcherUnhandledException(
        object sender,
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        ShowFatalError(e.Exception);

        e.Handled = true;
    }


    private static void ShowFatalError(Exception ex)
    {
        MessageBox.Show(
            ex.ToString(),
            "LuckyCharm - Startup Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}