using System;
using System.Threading;
using System.Windows;

namespace LuckyDangle;

public partial class App : Application
{
    private static Mutex? _singleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        const string mutexName = @"Global\LuckyCharm.SingleInstance";

        _singleInstanceMutex = new Mutex(
            initiallyOwned: true,
            name: mutexName,
            createdNew: out bool createdNew);

        if (!createdNew)
        {
            Shutdown();
            return;
        }

        base.OnStartup(e);

        StartupManager.EnsureEnabled();

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

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();

        base.OnExit(e);
    }
}