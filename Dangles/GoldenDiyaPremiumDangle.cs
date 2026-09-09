namespace LuckyDangle.Dangles;

public sealed class GoldenDiyaPremiumDangle : ImageDangleBase
{
    public override string Id => "indian_festival_golden_diya";
    public override string Name => "Golden Diya";
    public override string Description =>
        "A luminous Deepawali diya charm for light, warmth and auspicious beginnings.";
    public override string Category => "Festive";
    public override string Collection => "Indian Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/IndianFestivals/golden_diya.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}