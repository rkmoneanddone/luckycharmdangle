using System.IO;
using System.Text.Json;

namespace LuckyDangle.Services;

public sealed class PendingPremiumCheckout
{
    public string CheckoutId { get; set; } = "";
    public string CheckoutUrl { get; set; } = "";
    public string Email { get; set; } = "";
    public string Plan { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsRecent =>
        !string.IsNullOrWhiteSpace(CheckoutId) &&
        CreatedAtUtc > DateTime.UtcNow.AddDays(-2);
}

public static class PendingPremiumCheckoutStore
{
    private static readonly string DirectoryPath =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "LuckyDangle");

    private static readonly string FilePath =
        Path.Combine(
            DirectoryPath,
            "pending-premium-checkout.json");

    private static readonly JsonSerializerOptions Options =
        new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

    public static void Save(PendingPremiumCheckout pending)
    {
        Directory.CreateDirectory(DirectoryPath);

        File.WriteAllText(
            FilePath,
            JsonSerializer.Serialize(pending, Options));
    }

    public static PendingPremiumCheckout? Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return null;

            var value =
                JsonSerializer.Deserialize<PendingPremiumCheckout>(
                    File.ReadAllText(FilePath),
                    Options);

            if (value?.IsRecent == true)
                return value;

            Clear();
            return null;
        }
        catch
        {
            return null;
        }
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