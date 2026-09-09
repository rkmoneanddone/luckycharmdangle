namespace LuckyDangle.Dangles;

public sealed class ChristmasTreePremiumDangle : ImageDangleBase
{
    public override string Id => "western_festival_christmas_tree";
    public override string Name => "Christmas Tree & Gifts";
    public override string Description =>
        "A festive Christmas tree charm filled with ornaments, gifts and holiday sparkle.";
    public override string Category => "Festive";
    public override string Collection => "Western Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/WesternFestivals/christmas_tree.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}