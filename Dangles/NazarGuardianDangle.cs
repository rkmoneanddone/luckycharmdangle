namespace LuckyDangle.Dangles;

public sealed class NazarGuardianDangle : ImageDangleBase
{
    public override string Id =>
        "nazar_guardian";

    public override string Name =>
        "Nazar Guardian";

    public override string Description =>
        "A powerful Indian-inspired protection charm with a guardian face, bells, chillies and lemon.";

    public override string Category =>
        "Protection";

    public override string Collection =>
        "Lucky Charms";

    public override bool IsPremium => false;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/nazar_guardian.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        220;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}