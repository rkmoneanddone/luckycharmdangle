namespace LuckyDangle.Dangles;

public sealed class GoldenFortunePremiumDangle : ImageDangleBase
{
    public override string Id =>
        "premium_golden_fortune";

    public override string Name =>
        "Golden Fortune";

    public override string Description =>
        "A Premium fortune charm inspired by the classic lucky coin.";

    public override string Category =>
        "Luck";

    public override string Collection =>
        "Premium Collection";

    public override bool IsPremium =>
        true;

    public override bool RequiresPremiumAccess =>
        true;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/lucky_coin.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        220;

    public override double HangPointX =>
        ImageWidth / 2;
}