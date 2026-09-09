using System.Reflection;
using System.IO;
using System.Text.Json;

namespace LuckyDangle.Services;

public sealed class AppUpdateStatus
{
    public bool IsAvailable { get; init; }
    public Version InstalledVersion { get; init; } = new(0, 0, 0, 0);
    public Version LatestVersion { get; init; } = new(0, 0, 0, 0);
    public string StoreUrl { get; init; } = string.Empty;
}

public static class UpdateAvailabilityService
{
    private const string DefaultStoreUrl =
        "https://apps.microsoft.com/detail/9N11M525D0M9";

    private static readonly TimeSpan CheckInterval =
        TimeSpan.FromDays(1);

    private static readonly string CacheFile =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "LuckyDangle",
            "update-check.json");

    private sealed class UpdateCheckCache
    {
        public DateTime CheckedAtUtc { get; set; }
        public string LatestVersion { get; set; } = "0.0.0.0";
        public string StoreUrl { get; set; } = DefaultStoreUrl;
    }

    public static async Task<AppUpdateStatus> GetStatusAsync(
        CancellationToken cancellationToken = default)
    {
        Version installed = GetInstalledVersion();

        var saved = LoadCache();

        if (saved is not null &&
            DateTime.UtcNow - saved.CheckedAtUtc < CheckInterval)
        {
            return BuildStatus(
                installed,
                saved.LatestVersion,
                saved.StoreUrl);
        }

        try
        {
            var config =
                await RuntimeConfigService.GetAsync(cancellationToken);

            string latestVersion =
                config.Updates.LatestVersion?.Trim()
                ?? "0.0.0.0";

            string storeUrl =
                string.IsNullOrWhiteSpace(config.Updates.StoreUrl)
                    ? DefaultStoreUrl
                    : config.Updates.StoreUrl.Trim();

            SaveCache(
                new UpdateCheckCache
                {
                    CheckedAtUtc = DateTime.UtcNow,
                    LatestVersion = latestVersion,
                    StoreUrl = storeUrl
                });

            return BuildStatus(
                installed,
                latestVersion,
                storeUrl);
        }
        catch
        {
            return NoUpdate(installed);
        }
    }

    public static void OpenStore(string? storeUrl)
    {
        string target =
            string.IsNullOrWhiteSpace(storeUrl)
                ? DefaultStoreUrl
                : storeUrl.Trim();

        try
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo
                {
                    FileName = target,
                    UseShellExecute = true
                });
        }
        catch
        {
        }
    }

    private static AppUpdateStatus BuildStatus(
        Version installed,
        string? latestText,
        string? storeUrl)
    {
        if (!Version.TryParse(
                latestText?.Trim(),
                out Version? latest))
        {
            return NoUpdate(installed);
        }

        return new AppUpdateStatus
        {
            IsAvailable = latest > installed,
            InstalledVersion = installed,
            LatestVersion = latest,
            StoreUrl =
                string.IsNullOrWhiteSpace(storeUrl)
                    ? DefaultStoreUrl
                    : storeUrl.Trim()
        };
    }

    private static UpdateCheckCache? LoadCache()
    {
        try
        {
            if (!File.Exists(CacheFile))
                return null;

            string json = File.ReadAllText(CacheFile);

            return JsonSerializer.Deserialize<UpdateCheckCache>(json);
        }
        catch
        {
            return null;
        }
    }

    private static void SaveCache(UpdateCheckCache cache)
    {
        try
        {
            string? folder = Path.GetDirectoryName(CacheFile);

            if (!string.IsNullOrWhiteSpace(folder))
                Directory.CreateDirectory(folder);

            File.WriteAllText(
                CacheFile,
                JsonSerializer.Serialize(cache));
        }
        catch
        {
        }
    }

    private static AppUpdateStatus NoUpdate(Version installed)
    {
        return new AppUpdateStatus
        {
            IsAvailable = false,
            InstalledVersion = installed,
            LatestVersion = new Version(0, 0, 0, 0),
            StoreUrl = DefaultStoreUrl
        };
    }

    private static Version GetInstalledVersion()
    {
        try
        {
            var packageVersion =
                Windows.ApplicationModel.Package.Current.Id.Version;

            return new Version(
                packageVersion.Major,
                packageVersion.Minor,
                packageVersion.Build,
                packageVersion.Revision);
        }
        catch
        {
        }

        return Assembly
                   .GetExecutingAssembly()
                   .GetName()
                   .Version
               ?? new Version(0, 0, 0, 0);
    }
}