namespace LuckyDangle.Dangles;

public sealed class DhaakDhunuchiPremiumDangle : ImageDangleBase
{
    public override string Id => "indian_festival_dhaak_dhunuchi";
    public override string Name => "Dhaak & Dhunuchi";
    public override string Description =>
        "The rhythm, incense and celebration of Durga Puja in one festive charm.";
    public override string Category => "Festive";
    public override string Collection => "Indian Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/IndianFestivals/dhaak_dhunuchi.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}