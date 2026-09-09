using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LuckyDangle.Services;

public sealed class CoffeeCheckoutResponse
{
    public string CheckoutId { get; set; } = "";
    public string CheckoutUrl { get; set; } = "";
    public string Provider { get; set; } = "";
}

public sealed class CoffeeStatusResponse
{
    public string Status { get; set; } = "";
    public string Provider { get; set; } = "";
    public string Currency { get; set; } = "";
    public long Amount { get; set; }
    public string PaymentId { get; set; } = "";

    public bool IsPaid =>
        string.Equals(
            Status,
            "paid",
            StringComparison.OrdinalIgnoreCase);
}
public static class SupportPurchaseService
{
    private static readonly HttpClient Http = new();

    private const string BackendBaseUrl =
        "https://asia-south1-lucky-dangle.cloudfunctions.net";

    public static string GetMarketCode()
    {
#if DEBUG
        var testMarket =
            Environment.GetEnvironmentVariable(
                "LUCKYDANGLE_TEST_MARKET");

        if (string.Equals(
                testMarket,
                "IN",
                StringComparison.OrdinalIgnoreCase))
            return "IN";

        if (string.Equals(
                testMarket,
                "INTL",
                StringComparison.OrdinalIgnoreCase))
            return "INTL";
#endif

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

    public static async Task<CoffeeCheckoutResponse>
        CreateCheckoutAsync(
            string email,
            decimal amount,
            CancellationToken cancellationToken = default)
    {
        var body = new
        {
            type = "coffee",
            email = email.Trim(),
            amount,
            market = GetMarketCode()
        };

        using var response = await Http.PostAsJsonAsync(
            $"{BackendBaseUrl}/createCoffeeCheckout",
            body,
            cancellationToken);

        var raw =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                ExtractError(
                    raw,
                    "Unable to create coffee checkout."));

        return JsonSerializer.Deserialize<CoffeeCheckoutResponse>(
                   raw,
                   new JsonSerializerOptions
                   {
                       PropertyNameCaseInsensitive = true
                   })
               ?? new CoffeeCheckoutResponse();
    }

    public static async Task<CoffeeStatusResponse>
        GetStatusAsync(
            string checkoutId,
            CancellationToken cancellationToken = default)
    {
        using var response = await Http.GetAsync(
            $"{BackendBaseUrl}/coffeeStatus?checkoutId=" +
            Uri.EscapeDataString(checkoutId),
            cancellationToken);

        var raw =
            await response.Content.ReadAsStringAsync(
                cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException(
                ExtractError(
                    raw,
                    "Unable to check coffee payment status."));

        return JsonSerializer.Deserialize<CoffeeStatusResponse>(
                   raw,
                   new JsonSerializerOptions
                   {
                       PropertyNameCaseInsensitive = true
                   })
               ?? new CoffeeStatusResponse();
    }
    private static string ExtractError(
        string raw,
        string fallback)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);

            if (
                doc.RootElement.TryGetProperty(
                    "error",
                    out var error) &&
                error.ValueKind ==
                    JsonValueKind.String)
            {
                return error.GetString() ?? fallback;
            }
        }
        catch
        {
        }

        return fallback;
    }
}