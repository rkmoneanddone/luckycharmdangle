namespace LuckyDangle.Dangles;

public sealed class GaneshaSpiritualDangle : ImageDangleBase
{
    public override string Id =>
        "spiritual_ganesha";

    public override string Name =>
        "Shri Ganesha";

    public override string Description =>
        "A regal Shri Ganesha pendant crafted in gold with rich red and green enamel, gemstones, pearls and an ornate lotus setting.";

    public override string Category =>
        "Spiritual";

    public override string Collection =>
        "Spiritual";

    public override bool IsPremium => false;
public override bool IsSeasonal =>
        false;

    
#if DEBUG
    // Development-only Premium purchase-flow test.
    // Release builds keep this previously shipped dangle free.
    public override bool RequiresPremiumAccess => false;
#endif
protected override string AssetPath =>
        "Assets/Dangles/spiritual_ganesha.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        225;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}