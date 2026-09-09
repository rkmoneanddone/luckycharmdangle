namespace LuckyDangle.Dangles;

public sealed class DiwaliCrackersPremiumDangle : ImageDangleBase
{
    public override string Id =>
        "indian_festival_diwali_crackers";

    public override string Name =>
        "Diwali Crackers";

    public override string Description =>
        "A bright Deepawali fireworks charm celebrating festive lights, sparkle and joy.";

    public override string Category =>
        "Festive";

    public override string Collection =>
        "Indian Festivals";

    public override bool IsPremium =>
        true;

    public override bool RequiresPremiumAccess =>
        true;

    public override bool IsSeasonal =>
        true;

    protected override string AssetPath =>
        "Assets/Dangles/IndianFestivals/diwali_crackers.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        220;

    public override double HangPointX =>
        ImageWidth / 2;
}