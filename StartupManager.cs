using Microsoft.Win32;
using System.Reflection;

namespace LuckyDangle;

internal static class StartupManager
{
    private const string RunKey =
        @"Software\Microsoft\Windows\CurrentVersion\Run";

    private const string AppName = "LuckyCharm";

    public static void EnsureEnabled()
    {
        try
        {
            string? executablePath =
                Environment.ProcessPath;

            if (string.IsNullOrWhiteSpace(executablePath))
                return;

            using RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(
                    RunKey,
                    writable: true);

            if (key == null)
                return;

            string? existing =
                key.GetValue(AppName) as string;

            string desired =
                $"\"{executablePath}\"";

            if (!string.Equals(
                    existing,
                    desired,
                    StringComparison.OrdinalIgnoreCase))
            {
                key.SetValue(AppName, desired);
            }
        }
        catch
        {
            // Startup registration must never prevent LuckyCharm
            // from launching normally.
        }
    }
}