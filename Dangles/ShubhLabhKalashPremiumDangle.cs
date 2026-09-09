namespace LuckyDangle.Dangles;

public sealed class ShubhLabhKalashPremiumDangle : ImageDangleBase
{
    public override string Id => "indian_festival_shubh_labh_kalash";
    public override string Name => "Shubh Labh Kalash";
    public override string Description =>
        "An ornate Deepawali kalash carrying wishes of shubh, labh and abundance.";
    public override string Category => "Festive";
    public override string Collection => "Indian Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/IndianFestivals/shubh_labh_kalash.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}