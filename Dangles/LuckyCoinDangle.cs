namespace LuckyDangle.Dangles;

public sealed class LuckyCoinDangle : ImageDangleBase
{
    public override string Id =>
        "lucky_coin";

    public override string Name =>
        "Lucky Coin";

    public override string Description =>
        "An ornate Chinese lucky coin talisman with red cord, jade beads, gold accents and symbols of prosperity.";

    public override string Category =>
        "Luck";

    public override string Collection =>
        "Lucky Charms";

    public override bool IsPremium => false;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/lucky_coin.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        220;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}