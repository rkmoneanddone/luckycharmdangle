namespace LuckyDangle.Dangles;

public sealed class ThanksgivingTurkeyPremiumDangle : ImageDangleBase
{
    public override string Id => "western_festival_thanksgiving_turkey";
    public override string Name => "Thanksgiving Turkey";
    public override string Description =>
        "A warm Thanksgiving turkey charm with pumpkins, autumn leaves and harvest colors.";
    public override string Category => "Festive";
    public override string Collection => "Western Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/WesternFestivals/thanksgiving_turkey.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}