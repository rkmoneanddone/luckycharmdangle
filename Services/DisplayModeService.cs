using System;
using System.IO;
using System.Text.Json;

namespace LuckyDangle.Services;

public enum DangleDisplayMode
{
    DesktopOnly,
    AlwaysOnTop
}

public static class DisplayModeService
{
    private static readonly string SettingsFile =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "LuckyDangle",
            "display-mode.json");

    private static readonly string ExistingInstallMarker =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "LuckyDangle",
            "window-position.json");

    private sealed class DisplayModeSettings
    {
        public string Mode { get; set; } = string.Empty;
    }

    public static DangleDisplayMode Load()
    {
        try
        {
            if (File.Exists(SettingsFile))
            {
                var json = File.ReadAllText(SettingsFile);
                var settings =
                    JsonSerializer.Deserialize<DisplayModeSettings>(json);

                if (
                    settings != null &&
                    Enum.TryParse<DangleDisplayMode>(
                        settings.Mode,
                        true,
                        out var savedMode))
                {
                    return savedMode;
                }
            }
        }
        catch
        {
        }

        if (File.Exists(ExistingInstallMarker))
            return DangleDisplayMode.AlwaysOnTop;

        return DangleDisplayMode.DesktopOnly;
    }

    public static void Save(DangleDisplayMode mode)
    {
        try
        {
            var folder = Path.GetDirectoryName(SettingsFile)!;
            Directory.CreateDirectory(folder);

            File.WriteAllText(
                SettingsFile,
                JsonSerializer.Serialize(
                    new DisplayModeSettings
                    {
                        Mode = mode.ToString()
                    }));
        }
        catch
        {
        }
    }

    public static bool IsAlwaysOnTop =>
        Load() == DangleDisplayMode.AlwaysOnTop;
}