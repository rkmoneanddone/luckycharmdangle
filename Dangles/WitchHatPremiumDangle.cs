namespace LuckyDangle.Dangles;

public sealed class WitchHatPremiumDangle : ImageDangleBase
{
    public override string Id => "western_festival_witch_hat";
    public override string Name => "Witch Hat & Broom";
    public override string Description =>
        "A magical witch hat and broom charm with colorful Halloween accents.";
    public override string Category => "Festive";
    public override string Collection => "Western Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/WesternFestivals/witch_hat.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}