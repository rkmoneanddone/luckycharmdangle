namespace LuckyDangle.Dangles;

public sealed class KundanHeartDangle : ImageDangleBase
{
    public override string Id =>
        "rakhi_kundan_heart";

    public override string Name =>
        "Kundan Heart";

    public override string Description =>
        "An ornate kundan heart Rakhi adorned with rubies, emeralds, pearls and gold filigree.";

    public override string Category =>
    "Festive";

public override string Collection =>
    "Rakhi";

    public override bool IsPremium => false;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/rakhi_kundan_heart.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        210;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}