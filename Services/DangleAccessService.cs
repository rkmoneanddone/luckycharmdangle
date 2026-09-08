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
        return !dangle.RequiresPremiumAccess ||
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

    // Kept for development/testing compatibility.
    public static void SetPremiumEntitlementForCurrentSession(bool active)
    {
        hasActivePremiumEntitlement = active;
    }
}