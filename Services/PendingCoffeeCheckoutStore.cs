using System;
using System.IO;
using System.Text.Json;

namespace LuckyDangle.Services;

public sealed class PendingCoffeeCheckout
{
    public string CheckoutId { get; set; } = "";
    public string CheckoutUrl { get; set; } = "";
    public string Email { get; set; } = "";
    public decimal Amount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public static class PendingCoffeeCheckoutStore
{
    private static readonly JsonSerializerOptions JsonOptions =
        new() { WriteIndented = true };

    private static string FilePath
    {
        get
        {
            var root = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "LuckyDangle");

            Directory.CreateDirectory(root);

            return Path.Combine(
                root,
                "pending-coffee-checkout.json");
        }
    }

    public static void Save(PendingCoffeeCheckout pending)
    {
        File.WriteAllText(
            FilePath,
            JsonSerializer.Serialize(pending, JsonOptions));
    }

    public static PendingCoffeeCheckout? LoadRecent()
    {
        try
        {
            if (!File.Exists(FilePath))
                return null;

            var pending =
                JsonSerializer.Deserialize<PendingCoffeeCheckout>(
                    File.ReadAllText(FilePath));

            if (pending == null)
                return null;

            if (
                pending.CreatedAtUtc <= DateTime.MinValue ||
                DateTime.UtcNow - pending.CreatedAtUtc >
                    TimeSpan.FromDays(2))
            {
                Clear();
                return null;
            }

            return pending;
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