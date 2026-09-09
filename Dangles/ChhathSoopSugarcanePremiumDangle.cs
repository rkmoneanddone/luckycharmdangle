namespace LuckyDangle.Dangles;

public sealed class ChhathSoopSugarcanePremiumDangle : ImageDangleBase
{
    public override string Id => "indian_festival_chhath_soop_sugarcane";
    public override string Name => "Chhath Soop & Sugarcane";
    public override string Description =>
        "A festive Chhath offering charm with soop, fruits, sugarcane and devotion.";
    public override string Category => "Festive";
    public override string Collection => "Indian Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/IndianFestivals/chhath_soop_sugarcane.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}