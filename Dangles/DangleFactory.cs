using System;
using System.Collections.Generic;

namespace LuckyDangle.Dangles;

public static class DangleFactory
{
    public static IReadOnlyList<IDangle>
        GetAvailableDangles()
    {
        return DangleCatalog.GetAll();
    }


    public static IDangle?
        GetRandomDangle()
    {
        var dangles =
            DangleCatalog.GetAll();

        if (dangles.Count == 0)
            return null;

        return dangles[
            Random.Shared.Next(
                dangles.Count)];
    }


    public static IReadOnlyList<IDangle>
        GetFreeDangles()
    {
        return DangleCatalog.GetFree();
    }


    public static IReadOnlyList<IDangle>
        GetPremiumDangles()
    {
        return DangleCatalog.GetPremium();
    }


    public static IReadOnlyList<IDangle>
        GetSeasonalDangles()
    {
        return DangleCatalog.GetSeasonal();
    }


    public static IReadOnlyList<IDangle>
        GetDanglesByCategory(
            string category)
    {
        return DangleCatalog
            .GetCategory(category);
    }


    public static IReadOnlyList<string>
        GetCategories()
    {
        return DangleCatalog
            .GetCategories();
    }
}