using System.IO;
using System.Text.Json;

namespace LuckyDangle.Services;

public sealed class PremiumEntitlementSnapshot
{
    public string Email { get; set; } = "";
    public DateTime ExpiresAtUtc { get; set; }
    public string RestoreCode { get; set; } = "";

    public bool IsActive =>
        ExpiresAtUtc > DateTime.UtcNow;
}

public static class PremiumEntitlementStore
{
    private static readonly string FolderPath =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "LuckyDangle");

    private static readonly string FilePath =
        Path.Combine(FolderPath, "premium-entitlement.json");

    public static PremiumEntitlementSnapshot? Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return null;

            var json = File.ReadAllText(FilePath);
            var entitlement =
                JsonSerializer.Deserialize<PremiumEntitlementSnapshot>(json);

            if (entitlement is null || !entitlement.IsActive)
            {
                Clear();
                return null;
            }

            return entitlement;
        }
        catch
        {
            return null;
        }
    }

    public static void Save(PremiumEntitlementSnapshot entitlement)
    {
        Directory.CreateDirectory(FolderPath);

        var json = JsonSerializer.Serialize(
            entitlement,
            new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(FilePath, json);
    }

    public static void Clear()
    {
        try
        {
            if (File.Exists(FilePath))
                File.Delete(FilePath);
        }
        catch
        {
        }
    }
}