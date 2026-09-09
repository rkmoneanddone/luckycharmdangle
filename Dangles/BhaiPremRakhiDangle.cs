namespace LuckyDangle.Dangles;

public sealed class BhaiPremRakhiDangle : ImageDangleBase
{
    public override string Id =>
        "rakhi_bhai_prem";

    public override string Name =>
        "Bhai Prem Rakhi";

    public override string Description =>
        "A luxurious red and gold Rakhi celebrating the love between brother and sister, adorned with pearls, gemstones and a Best Brother Ever charm.";

    public override string Category =>
        "Festive";

    public override string Collection =>
        "Rakhi";

    public override bool IsPremium => false;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/rakhi_bhai_prem.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        225;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}