namespace LuckyDangle.Dangles;

public sealed class LuckyElephantDangle : ImageDangleBase
{
    public override string Id =>
        "rakhi_lucky_elephant";

    public override string Name =>
        "Lucky Elephant";

    public override string Description =>
        "A royal elephant Rakhi charm adorned with gemstones, pearls and ornate gold detailing.";

    public override string Category =>
    "Luck";

public override string Collection =>
    "Rakhi";

    public override bool IsPremium =>
        true;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/rakhi_lucky_elephant.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        210;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}