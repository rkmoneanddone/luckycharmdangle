namespace LuckyDangle.Dangles;

public sealed class EvilEyeShieldDangle : ImageDangleBase
{
    public override string Id =>
        "rakhi_evil_eye";

    public override string Name =>
        "Evil Eye Shield";

    public override string Description =>
        "A premium Nazar Rakhi adorned with blue gemstones, pearls and gold detailing.";

    public override string Category =>
    "Protection";

public override string Collection =>
    "Lucky Charms";

    public override bool IsPremium => false;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/rakhi_evil_eye.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        210;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}