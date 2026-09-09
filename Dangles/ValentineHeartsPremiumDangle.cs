namespace LuckyDangle.Dangles;

public sealed class ValentineHeartsPremiumDangle : ImageDangleBase
{
    public override string Id => "western_festival_valentine_hearts";
    public override string Name => "Valentine Hearts";
    public override string Description =>
        "A romantic heart charm celebrating love, affection and Valentine's Day.";
    public override string Category => "Festive";
    public override string Collection => "Western Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/WesternFestivals/valentine_hearts.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}