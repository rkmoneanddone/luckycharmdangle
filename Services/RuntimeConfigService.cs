using System.Net.Http;
using System.Text.Json;

namespace LuckyDangle.Services;

public sealed class CoffeeRuntimeConfig
{
    public decimal IndiaDefault { get; set; } = 200m;
    public decimal[] IndiaPresets { get; set; } = [100m, 150m, 200m];
    public decimal IndiaMin { get; set; } = 100m;
    public decimal IndiaMax { get; set; } = 5000m;
    public decimal InternationalDefault { get; set; } = 3m;
    public decimal[] InternationalPresets { get; set; } = [3m, 5m, 10m];
    public decimal InternationalMin { get; set; } = 3m;
    public decimal InternationalMax { get; set; } = 100m;
}

public sealed class PremiumRuntimeConfig
{
    public decimal India6mMin { get; set; } = 299m;
    public decimal India12mMin { get; set; } = 449m;
    public decimal IndiaMax { get; set; } = 19999m;
    public decimal International6mMin { get; set; } = 6m;
    public decimal International12mMin { get; set; } = 9m;
    public decimal InternationalMax { get; set; } = 200m;
}

public sealed class ProviderRuntimeConfig
{
    public bool RazorpayEnabled { get; set; } = false;
    public bool DodoEnabled { get; set; } = true;
}

public sealed class UpdateRuntimeConfig
{
    // Fail-safe client default: no Firebase value means no update indicator.
    public string LatestVersion { get; set; } = "0.0.0.0";
    public string StoreUrl { get; set; } =
        "https://apps.microsoft.com/detail/9N11M525D0M9";
}

public sealed class LuckyDanglePublicConfig
{
    public string SupportEmail { get; set; } = "connect@quickstories.in";
    public string PrivacyUrl { get; set; } =
        "https://www.quickstories.in/lucky-dangle-privacy.html";    public string RefundPolicyText { get; set; } =
        "Lucky Dangle Premium purchases are generally non-refundable. " +
        "Please verify the selected plan, email address, and payment amount " +
        "before completing payment. Refunds will only be provided where " +
        "required by applicable law. For payment or access issues, contact " +
        "connect@quickstories.in.";
    public CoffeeRuntimeConfig Coffee { get; set; } = new();
    public PremiumRuntimeConfig Premium { get; set; } = new();
    public ProviderRuntimeConfig Providers { get; set; } = new();
    public UpdateRuntimeConfig Updates { get; set; } = new();
}

public static class RuntimeConfigService
{
    private const string BackendBaseUrl =
        "https://asia-south1-lucky-dangle.cloudfunctions.net";

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private static LuckyDanglePublicConfig? cached;
    private static DateTime cachedAtUtc;

    public static async Task<LuckyDanglePublicConfig> GetAsync(
        CancellationToken cancellationToken = default)
    {
        if (cached is not null &&
            DateTime.UtcNow - cachedAtUtc < TimeSpan.FromHours(6))
        {
            return cached;
        }

        try
        {
            using var response = await Http.GetAsync(
                $"{BackendBaseUrl}/getPublicConfig",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return cached ?? new LuckyDanglePublicConfig();

            var raw =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            cached =
                JsonSerializer.Deserialize<LuckyDanglePublicConfig>(
                    raw,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    })
                ?? new LuckyDanglePublicConfig();

            cachedAtUtc = DateTime.UtcNow;
            return cached;
        }
        catch
        {
            return cached ?? new LuckyDanglePublicConfig();
        }
    }
}

