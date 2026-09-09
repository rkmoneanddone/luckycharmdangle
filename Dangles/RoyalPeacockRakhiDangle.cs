namespace LuckyDangle.Dangles;

public sealed class RoyalPeacockRakhiDangle : ImageDangleBase
{
    public override string Id =>
        "rakhi_royal_peacock";

    public override string Name =>
        "Royal Peacock";

    public override string Description =>
        "An ornate royal peacock Rakhi adorned with colorful gemstones, pearls and gold detailing.";

    public override string Category =>
        "Festive";

    public override string Collection =>
        "Rakhi";

    public override bool IsPremium => true;

    public override bool RequiresPremiumAccess => true;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/rakhi_royal_peacock.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        220;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}