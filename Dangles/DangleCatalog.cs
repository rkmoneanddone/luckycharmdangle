using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

    private static readonly List<IDangle> BuiltInDangles =
    new()
    {
        // =================================================
        // RAKHI COLLECTION
        // =================================================

        new DivineRakhiDangle(),
        new LotusGraceDangle(),
        new ShreeGaneshaDangle(),
        new KundanHeartDangle(),
        new RoyalPeacockRakhiDangle(),
        new LuckyElephantDangle(),
        new BhaiMereHeroRakhiDangle(),
        new BhaiPremRakhiDangle(),
        new BhabhiPremRakhiDangle(),
        new BehenKaPyaarRakhiDangle(),

        // =================================================
            // SPIRITUAL COLLECTION
        // =================================================

        new OmSpiritualDangle(),
        new ShreeYantraSpiritualDangle(),
        new TrishulSpiritualDangle(),
        new LotusPadmaSpiritualDangle(),
        new KrishnaSpiritualDangle(),
        new GaneshaSpiritualDangle(),
        new LakshmiSpiritualDangle(),

        // =================================================
        // LUCKY CHARMS COLLECTION
        // =================================================

        new EvilEyeShieldDangle(),
        new LuckyCoinDangle(),
        new ManekiNekoDangle(),
        new NimbuMirchiDangle(),
        new NazarGuardianDangle(),
        new HamsaKhamsaDangle(),
        new CornicelloDangle(),
        new JetStoneHiguerillaDangle(),
    };

    private static readonly List<IDangle> AllDangles =
        BuildCatalog();

    private static List<IDangle> BuildCatalog()
    {
        var result =
            new List<IDangle>(BuiltInDangles);

        var knownTypes =
            BuiltInDangles
                .Select(d => d.GetType())
                .ToHashSet();

        var discovered =
            typeof(IDangle)
                .Assembly
                .GetTypes()
                .Where(type =>
                    !type.IsAbstract &&
                    !type.IsInterface &&
                    typeof(IDangle).IsAssignableFrom(type) &&
                    type.GetConstructor(Type.EmptyTypes) is not null &&
                    !knownTypes.Contains(type))
                .Select(type =>
                    (IDangle)Activator.CreateInstance(type)!)
                .OrderBy(d => d.Collection)
                .ThenBy(d => d.Name)
                .ToList();

        result.AddRange(discovered);
        return result;
    }

    public static IReadOnlyList<IDangle> GetAll() => AllDangles;

    public static IReadOnlyList<IDangle> GetFree() =>
        AllDangles.Where(d => !d.IsPremium).ToList();

    public static IReadOnlyList<IDangle> GetPremium() =>
        AllDangles.Where(d => d.IsPremium).ToList();

    public static IReadOnlyList<IDangle> GetSeasonal() =>
        AllDangles.Where(d => d.IsSeasonal).ToList();

    public static IReadOnlyList<IDangle> GetCategory(string category) =>
        AllDangles
            .Where(d => d.Category.Equals(
                category,
                System.StringComparison.OrdinalIgnoreCase))
            .ToList();

    public static IReadOnlyList<string> GetCategories() =>
        AllDangles
            .Select(d => d.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

    public static IDangle? Find(string name) =>
        AllDangles.FirstOrDefault(d => d.Name.Equals(
            name,
            System.StringComparison.OrdinalIgnoreCase));
}
