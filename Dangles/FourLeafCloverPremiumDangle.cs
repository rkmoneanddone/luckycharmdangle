namespace LuckyDangle.Dangles;

public sealed class FourLeafCloverPremiumDangle : ImageDangleBase
{
    public override string Id =>
        "four_leaf_clover";

    public override string Name =>
        "Four-Leaf Clover";

    public override string Description =>
        "A classic four-leaf clover charm symbolizing luck, hope, faith and fortune.";

    public override string Category =>
        "Luck";

    public override string Collection =>
        "Lucky Charms";

    public override bool IsPremium =>
        true;

    public override bool RequiresPremiumAccess =>
        true;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/four_leaf_clover.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        220;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}