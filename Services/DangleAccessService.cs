using LuckyDangle.Dangles;

namespace LuckyDangle.Services;

public static class DangleAccessService
{
    private static bool hasActivePremiumEntitlement =
        PremiumEntitlementStore.Load()?.IsActive == true;

    public static bool HasActivePremiumEntitlement
    {
        get
        {
            if (hasActivePremiumEntitlement)
            {
                var local = PremiumEntitlementStore.Load();
                hasActivePremiumEntitlement =
                    local?.IsActive == true;
            }

            return hasActivePremiumEntitlement;
        }
    }

    public static bool CanUse(IDangle dangle)
    {
        return !dangle.IsPremium ||
               HasActivePremiumEntitlement;
    }

    public static IReadOnlyList<IDangle> GetUsableDangles()
    {
        return DangleCatalog.GetAll()
            .Where(CanUse)
            .ToList();
    }

    public static void ApplyPremiumEntitlement(
        PremiumEntitlementSnapshot entitlement)
    {
        PremiumEntitlementStore.Save(entitlement);
        hasActivePremiumEntitlement = entitlement.IsActive;
    }

    public static void RefreshFromLocalStorage()
    {
        hasActivePremiumEntitlement =
            PremiumEntitlementStore.Load()?.IsActive == true;
    }
    public static async Task RevalidateIfDueAsync(
        CancellationToken cancellationToken = default)
    {
        var local = PremiumEntitlementStore.Load();

        if (local is null || !local.IsActive)
        {
            hasActivePremiumEntitlement = false;
            return;
        }

        hasActivePremiumEntitlement = true;

        if (string.IsNullOrWhiteSpace(local.ValidationToken))
        {
            // Existing installs from before validation-token support
            // remain locally valid until expiry. A future purchase or
            // OTP restore will issue the validation token.
            return;
        }

        if (
            local.LastServerValidationAtUtc.HasValue &&
            DateTime.UtcNow -
                local.LastServerValidationAtUtc.Value <
                TimeSpan.FromDays(7))
        {
            return;
        }

        try
        {
            var status =
                await PremiumPurchaseService
                    .RevalidateEntitlementAsync(
                        local.Email,
                        local.ValidationToken,
                        cancellationToken);

            if (!status.IsActive ||
                !status.ExpiresAtUtc.HasValue)
            {
                PremiumEntitlementStore.Clear();
                hasActivePremiumEntitlement = false;
                return;
            }

            local.Email = status.Email;
            local.ExpiresAtUtc =
                status.ExpiresAtUtc.Value;
            local.LastServerValidationAtUtc =
                DateTime.UtcNow;

            PremiumEntitlementStore.Save(local);
            hasActivePremiumEntitlement = true;
        }
        catch
        {
            // Network/server errors must not revoke an otherwise
            // locally valid entitlement. Leave timestamp unchanged
            // so a later startup can retry.
        }
    }

    // Kept for development/testing compatibility.
    public static void SetPremiumEntitlementForCurrentSession(bool active)
    {
        hasActivePremiumEntitlement = active;
    }
}