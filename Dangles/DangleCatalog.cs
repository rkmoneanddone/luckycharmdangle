using System.Collections.Generic;
using System.Linq;

namespace LuckyDangle.Dangles;

public static class DangleCatalog
{
    // =====================================================
    // MASTER DANGLER LIST
    //
    // Add a new dangle HERE only.
    //
    // The picker, categories, random selection etc.
    // will use this automatically.
    // =====================================================

    private static readonly List<IDangle> AllDangles =
    new()
    {
        new RakhiDangle(),
        new RoyalRakhiDangle(),
        new PeacockRakhiDangle(),
        new LuckyCoinDangle(),
        new ManekiNekoDangle()
    };


    // =====================================================
    // ALL DANGLERS
    // =====================================================

    public static IReadOnlyList<IDangle> GetAll()
    {
        return AllDangles;
    }


    // =====================================================
    // FREE DANGLERS
    // =====================================================

    public static IReadOnlyList<IDangle> GetFree()
    {
        return AllDangles
            .Where(d => !d.IsPremium)
            .ToList();
    }


    // =====================================================
    // PREMIUM DANGLERS
    // =====================================================

    public static IReadOnlyList<IDangle> GetPremium()
    {
        return AllDangles
            .Where(d => d.IsPremium)
            .ToList();
    }


    // =====================================================
    // SEASONAL DANGLERS
    // =====================================================

    public static IReadOnlyList<IDangle> GetSeasonal()
    {
        return AllDangles
            .Where(d => d.IsSeasonal)
            .ToList();
    }


    // =====================================================
    // CATEGORY
    // =====================================================

    public static IReadOnlyList<IDangle> GetCategory(
        string category)
    {
        return AllDangles
            .Where(d =>
                d.Category.Equals(
                    category,
                    System.StringComparison.OrdinalIgnoreCase))
            .ToList();
    }


    // =====================================================
    // CATEGORY NAMES
    // =====================================================

    public static IReadOnlyList<string> GetCategories()
    {
        return AllDangles
            .Select(d => d.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }


    // =====================================================
    // FIND BY NAME
    // =====================================================

    public static IDangle? Find(
        string name)
    {
        return AllDangles.FirstOrDefault(
            d => d.Name.Equals(
                name,
                System.StringComparison.OrdinalIgnoreCase));
    }
}