using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace LuckyDangle.Services;

public sealed class PremiumCheckoutResponse
{
    public string CheckoutId { get; set; } = "";
    public string CheckoutUrl { get; set; } = "";
    public string Provider { get; set; } = "";
}

public sealed class PremiumStatusResponse
{
    public string Status { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime? ExpiresAtUtc { get; set; }
    public string RestoreCode { get; set; } = "";

    public bool IsActive =>
        string.Equals(Status, "active",
            StringComparison.OrdinalIgnoreCase) &&
        ExpiresAtUtc.HasValue &&
        ExpiresAtUtc.Value > DateTime.UtcNow;
}

public static class PremiumPurchaseService
{
    public const string BackendBaseUrl =
        "https://asia-south1-lucky-dangle.cloudfunctions.net";

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    public static string GetMarketCode()
    {
        try
        {
            return string.Equals(
                RegionInfo.CurrentRegion.TwoLetterISORegionName,
                "IN",
                StringComparison.OrdinalIgnoreCase)
                ? "IN"
                : "INTL";
        }
        catch
        {
            return "INTL";
        }
    }

    public static async Task<PremiumCheckoutResponse> CreateCheckoutAsync(string email, string plan, decimal amount, CancellationToken cancellationToken = default)
    {
        var body = new
        {
            email = email.Trim(),
            plan,
            amount,
            market = GetMarketCode()
        };

        using var response = await Http.PostAsJsonAsync(
            $"{BackendBaseUrl}/createPremiumCheckout",
            body,
            cancellationToken);

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                ExtractError(raw, "Unable to start checkout."));

        return JsonSerializer.Deserialize<PremiumCheckoutResponse>(
                   raw,
                   JsonOptions())
               ?? throw new InvalidOperationException(
                   "Invalid checkout response.");
    }

    public static void OpenCheckout(string checkoutUrl)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = checkoutUrl,
            UseShellExecute = true
        });
    }

    public static async Task<PremiumStatusResponse> GetStatusAsync(
        string checkoutId,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"{BackendBaseUrl}/premiumStatus?checkoutId=" +
            Uri.EscapeDataString(checkoutId);

        using var response =
            await Http.GetAsync(url, cancellationToken);

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                ExtractError(raw, "Unable to check payment status."));

        return JsonSerializer.Deserialize<PremiumStatusResponse>(
                   raw,
                   JsonOptions())
               ?? new PremiumStatusResponse();
    }

    public static async Task SendRestoreOtpAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            email = email.Trim()
        };

        using var response = await Http.PostAsJsonAsync(
            $"{BackendBaseUrl}/sendRestoreOtp",
            body,
            cancellationToken);

        var raw =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                ExtractError(
                    raw,
                    "Unable to send verification code."));
    }

    public static async Task<PremiumStatusResponse>
        VerifyRestoreOtpAsync(
            string email,
            string code,
            CancellationToken cancellationToken = default)
    {
        var body = new
        {
            email = email.Trim(),
            code = code.Trim()
        };

        using var response = await Http.PostAsJsonAsync(
            $"{BackendBaseUrl}/verifyRestoreOtp",
            body,
            cancellationToken);

        var raw =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                ExtractError(
                    raw,
                    "Premium could not be restored."));

        return JsonSerializer.Deserialize<PremiumStatusResponse>(
                   raw,
                   JsonOptions())
               ?? new PremiumStatusResponse();
    }
    public static async Task<PremiumStatusResponse> RestoreAsync(
        string email,
        string restoreCode,
        CancellationToken cancellationToken = default)
    {
        var body = new
        {
            email = email.Trim(),
            restoreCode = restoreCode.Trim()
        };

        using var response = await Http.PostAsJsonAsync(
            $"{BackendBaseUrl}/restorePremium",
            body,
            cancellationToken);

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                ExtractError(raw, "Premium could not be restored."));

        return JsonSerializer.Deserialize<PremiumStatusResponse>(
                   raw,
                   JsonOptions())
               ?? new PremiumStatusResponse();
    }

    private static JsonSerializerOptions JsonOptions() =>
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    private static string ExtractError(string raw, string fallback)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            if (doc.RootElement.TryGetProperty("error", out var error))
                return error.GetString() ?? fallback;
        }
        catch
        {
        }

        return fallback;
    }
}