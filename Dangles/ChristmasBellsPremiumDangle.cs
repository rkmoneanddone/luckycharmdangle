namespace LuckyDangle.Dangles;

public sealed class ChristmasBellsPremiumDangle : ImageDangleBase
{
    public override string Id => "western_festival_christmas_bells";
    public override string Name => "Christmas Bell & Holly";
    public override string Description =>
        "Golden Christmas bells with holly, ribbon and sparkling festive details.";
    public override string Category => "Festive";
    public override string Collection => "Western Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/WesternFestivals/christmas_bells.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}