namespace LuckyDangle.Dangles;

public sealed class MaaDurgaShaktiPremiumDangle : ImageDangleBase
{
    public override string Id => "indian_festival_maa_durga_shakti";
    public override string Name => "Maa Durga Shakti";
    public override string Description =>
        "A radiant Durga Puja charm celebrating Maa Durga, shakti and festive devotion.";
    public override string Category => "Festive";
    public override string Collection => "Indian Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/IndianFestivals/maa_durga_shakti.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}