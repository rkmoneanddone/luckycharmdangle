namespace LuckyDangle.Dangles;

public sealed class NimbuMirchiDangle : ImageDangleBase
{
    public override string Id =>
        "nimbu_mirchi";

    public override string Name =>
        "Nimbu Mirchi";

    public override string Description =>
        "A traditional Indian nimbu mirchi protection charm with chillies, lemon, bells and Nazar.";

    public override string Category =>
    "Protection";

public override string Collection =>
    "Lucky Charms";

    public override bool IsPremium => true;

    public override bool RequiresPremiumAccess => true;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/rakhi_nimbu_mirchi.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        210;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}