namespace LuckyDangle.Dangles;

public sealed class TrishulSpiritualDangle : ImageDangleBase
{
    public override string Id =>
        "spiritual_trishul";

    public override string Name =>
        "Trishul";

    public override string Description =>
        "A powerful yet elegant Trishul pendant crafted in gold with crimson enamel, gemstone detailing, damru and Rudraksha-inspired accents.";

    public override string Category =>
        "Spiritual";

    public override string Collection =>
        "Spiritual";

    public override bool IsPremium =>
        true;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/spiritual_trishul.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        225;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}