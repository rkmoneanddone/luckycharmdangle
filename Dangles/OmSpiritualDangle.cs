namespace LuckyDangle.Dangles;

public sealed class OmSpiritualDangle : ImageDangleBase
{
    public override string Id =>
        "spiritual_om";

    public override string Name =>
        "Om";

    public override string Description =>
        "An elegant Om pendant crafted in gold with red enamel, gemstone detailing and a refined devotional design.";

    public override string Category =>
        "Spiritual";

    public override string Collection =>
        "Spiritual";

    public override bool IsPremium => false;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/spiritual_om.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        225;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}