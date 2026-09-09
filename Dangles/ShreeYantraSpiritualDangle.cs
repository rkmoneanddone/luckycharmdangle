namespace LuckyDangle.Dangles;

public sealed class ShreeYantraSpiritualDangle : ImageDangleBase
{
    public override string Id =>
        "spiritual_shree_yantra";

    public override string Name =>
        "Shree Yantra";

    public override string Description =>
        "An elegant Shree Yantra pendant crafted in gold with rich red enamel, gemstones and lotus-inspired detailing.";

    public override string Category =>
        "Spiritual";

    public override string Collection =>
        "Spiritual";

    public override bool IsPremium => true;

    public override bool RequiresPremiumAccess => true;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/spiritual_shree_yantra.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        225;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}