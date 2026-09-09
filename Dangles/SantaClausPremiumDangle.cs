namespace LuckyDangle.Dangles;

public sealed class SantaClausPremiumDangle : ImageDangleBase
{
    public override string Id => "western_festival_santa_claus";
    public override string Name => "Santa Claus";
    public override string Description =>
        "A cheerful Santa Claus charm carrying gifts and classic Christmas joy.";
    public override string Category => "Festive";
    public override string Collection => "Western Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/WesternFestivals/santa_claus.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}