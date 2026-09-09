namespace LuckyDangle.Dangles;

public sealed class HalloweenPumpkinPremiumDangle : ImageDangleBase
{
    public override string Id => "western_festival_halloween_pumpkin";
    public override string Name => "Halloween Pumpkin";
    public override string Description =>
        "A glowing Halloween pumpkin charm with playful spooky-season details.";
    public override string Category => "Festive";
    public override string Collection => "Western Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/WesternFestivals/halloween_pumpkin.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}