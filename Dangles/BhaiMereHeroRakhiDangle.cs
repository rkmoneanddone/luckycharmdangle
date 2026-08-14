namespace LuckyDangle.Dangles;

public sealed class BhaiMereHeroRakhiDangle : ImageDangleBase
{
    public override string Id =>
        "bhai_mere_hero_rakhi";

    public override string Name =>
        "Bhai Mere Hero";

    public override string Description =>
        "A vibrant premium Rakhi featuring colorful gemstones, pearls, gold detailing, lotus and Nazar charms, and a red-gold tassel.";

    public override string Category =>
        "Festive";

    public override string Collection =>
        "Rakhi";

    public override bool IsPremium =>
        true;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/bhai_mere_hero_rakhi.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        220;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}