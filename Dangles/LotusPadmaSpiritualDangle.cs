namespace LuckyDangle.Dangles;

public sealed class LotusPadmaSpiritualDangle : ImageDangleBase
{
    public override string Id =>
        "spiritual_lotus_padma";

    public override string Name =>
        "Lotus Padma";

    public override string Description =>
        "An elegant Lotus Padma pendant crafted in gold with deep pink gemstones, pearls, diamond accents and graceful floral detailing.";

    public override string Category =>
        "Spiritual";

    public override string Collection =>
        "Spiritual";

    public override bool IsPremium =>
        true;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/spiritual_lotus_padma.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        225;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}