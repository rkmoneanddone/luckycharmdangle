namespace LuckyDangle.Dangles;

public sealed class BehenKaPyaarRakhiDangle : ImageDangleBase
{
    public override string Id =>
        "rakhi_behen_ka_pyaar";

    public override string Name =>
        "Behen Ka Pyaar";

    public override string Description =>
        "An elegant premium Rakhi celebrating the timeless love between a sister and brother, adorned with pearls, floral details, gemstones and a protective evil-eye charm.";

    public override string Category =>
        "Festive";

    public override string Collection =>
        "Rakhi";

    public override bool IsPremium =>
        true;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/rakhi_behen_ka_pyaar.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        225;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}