namespace LuckyDangle.Dangles;

public sealed class ShreeGaneshaDangle : ImageDangleBase
{
    public override string Id =>
        "rakhi_shree_ganesha";

    public override string Name =>
        "Shree Ganesha";

    public override string Description =>
        "A premium Ganesha Rakhi adorned with gold, gemstones and pearls.";

   public override string Category =>
    "Spiritual";

public override string Collection =>
    "Rakhi";

    public override bool IsPremium => false;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/rakhi_shree_ganesha.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        210;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}