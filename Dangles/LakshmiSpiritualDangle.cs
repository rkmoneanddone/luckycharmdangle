namespace LuckyDangle.Dangles;

public sealed class LakshmiSpiritualDangle : ImageDangleBase
{
    public override string Id =>
        "spiritual_lakshmi";

    public override string Name =>
        "Maa Lakshmi";

    public override string Description =>
        "A regal Maa Lakshmi pendant crafted in gold with pink lotus motifs, emeralds, rubies, pearls and elegant devotional detailing.";

    public override string Category =>
        "Spiritual";

    public override string Collection =>
        "Spiritual";

    public override bool IsPremium =>
        true;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/spiritual_lakshmi.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        225;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}