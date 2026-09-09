namespace LuckyDangle.Dangles;

public sealed class KrishnaSpiritualDangle : ImageDangleBase
{
    public override string Id =>
        "spiritual_krishna";

    public override string Name =>
        "Lord Krishna";

    public override string Description =>
        "A richly detailed Krishna-inspired pendant featuring a serene blue Krishna playing the flute, adorned with pearls, gemstones, gold filigree and a peacock feather.";

    public override string Category =>
        "Spiritual";

    public override string Collection =>
        "Spiritual";

    public override bool IsPremium => true;

    public override bool RequiresPremiumAccess => true;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/spiritual_krishna.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        225;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}