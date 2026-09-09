namespace LuckyDangle.Dangles;

public sealed class LotusGraceDangle : ImageDangleBase
{
    public override string Id =>
        "rakhi_lotus_grace";

    public override string Name =>
        "Lotus Grace";

    public override string Description =>
        "A graceful lotus Rakhi adorned with pink gemstones, pearls and gold detailing.";

    public override string Category =>
    "Festive";

public override string Collection =>
    "Rakhi";

    public override bool IsPremium => false;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/rakhi_lotus_grace.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        210;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}