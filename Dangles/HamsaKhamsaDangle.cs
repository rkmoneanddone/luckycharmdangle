namespace LuckyDangle.Dangles;

public sealed class HamsaKhamsaDangle : ImageDangleBase
{
    public override string Id =>
        "hamsa_khamsa";

    public override string Name =>
        "Hamsa / Khamsa";

    public override string Description =>
        "A jeweled Hamsa protection talisman with a Nazar eye, blue gemstones and gold filigree.";

   public override string Category =>
    "Protection";

public override string Collection =>
    "Lucky Charms";

    public override bool IsPremium => true;

    public override bool RequiresPremiumAccess => true;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/hamsa_khamsa.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        220;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}