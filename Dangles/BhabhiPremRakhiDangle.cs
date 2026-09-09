namespace LuckyDangle.Dangles;

public sealed class BhabhiPremRakhiDangle : ImageDangleBase
{
    public override string Id =>
        "rakhi_bhabhi_prem";

    public override string Name =>
        "Bhabhi Prem Rakhi";

    public override string Description =>
        "A luxurious pink and gold Rakhi celebrating the love, respect and family bond between a sister-in-law and her brother-in-law.";

    public override string Category =>
        "Festive";

    public override string Collection =>
        "Rakhi";

    public override bool IsPremium => false;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/rakhi_bhabhi_prem.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        225;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}