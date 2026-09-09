namespace LuckyDangle.Dangles;

public sealed class SnowmanPremiumDangle : ImageDangleBase
{
    public override string Id => "western_festival_snowman";
    public override string Name => "Snowman";
    public override string Description =>
        "A cheerful winter snowman charm with bright Christmas details.";
    public override string Category => "Festive";
    public override string Collection => "Western Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/WesternFestivals/snowman.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}