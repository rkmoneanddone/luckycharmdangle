namespace LuckyDangle.Dangles;

public sealed class NewYearPremiumDangle : ImageDangleBase
{
    public override string Id => "western_festival_new_year";
    public override string Name => "New Year Fireworks";
    public override string Description =>
        "A sparkling New Year charm with midnight celebration, fireworks and festive shine.";
    public override string Category => "Festive";
    public override string Collection => "Western Festivals";
    public override bool IsPremium => true;
    public override bool RequiresPremiumAccess => true;
    public override bool IsSeasonal => true;
    protected override string AssetPath =>
        "Assets/Dangles/WesternFestivals/new_year.png";
    protected override double ImageWidth => 145;
    protected override double ImageHeight => 220;
    public override double HangPointX => ImageWidth / 2;
}