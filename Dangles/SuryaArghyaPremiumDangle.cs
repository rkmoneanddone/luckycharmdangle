namespace LuckyDangle.Dangles;

public sealed class SuryaArghyaPremiumDangle : ImageDangleBase
{
    public override string Id => "indian_festival_surya_arghya";
    public override string Name => "Surya Arghya";
    public override string Description =>
        "A Chhath Puja charm honouring Surya Dev and the sacred arghya offering.";
    public override string Category => "Festive";
    public override string Collection => "Indian Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/IndianFestivals/surya_arghya.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}