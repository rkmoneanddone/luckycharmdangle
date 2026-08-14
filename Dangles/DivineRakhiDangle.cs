namespace LuckyDangle.Dangles;

public sealed class DivineRakhiDangle : ImageDangleBase
{
    public override string Id =>
        "rakhi_divine";

    public override string Name =>
        "Divine Rakhi";

    public override string Description =>
        "A radiant Om-inspired Rakhi with ruby gemstones, pearls and gold detailing.";

  public override string Category =>
    "Festive";

public override string Collection =>
    "Rakhi";

    public override bool IsPremium =>
        false;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/rakhi_divine.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        210;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}