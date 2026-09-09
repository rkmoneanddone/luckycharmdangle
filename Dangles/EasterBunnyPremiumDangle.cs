namespace LuckyDangle.Dangles;

public sealed class EasterBunnyPremiumDangle : ImageDangleBase
{
    public override string Id => "western_festival_easter_bunny";
    public override string Name => "Easter Bunny & Eggs";
    public override string Description =>
        "A cheerful Easter bunny charm surrounded by decorated eggs and spring flowers.";
    public override string Category => "Festive";
    public override string Collection => "Western Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/WesternFestivals/easter_bunny.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}