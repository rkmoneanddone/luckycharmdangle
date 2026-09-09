namespace LuckyDangle.Dangles;

public sealed class LakshmiCharanPremiumDangle : ImageDangleBase
{
    public override string Id => "indian_festival_lakshmi_charan";
    public override string Name => "Lakshmi Charan";
    public override string Description =>
        "Sacred Lakshmi footprints on lotus, symbolising prosperity and blessings.";
    public override string Category => "Festive";
    public override string Collection => "Indian Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/IndianFestivals/lakshmi_charan.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}