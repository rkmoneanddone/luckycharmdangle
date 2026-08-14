namespace LuckyDangle.Dangles;

using System.Windows.Media;

public class ManekiNekoDangle : ImageDangleBase
{
    public override string Id =>
        "maneki_neko";

    public override string Name =>
        "Maneki Neko";

    public override string Description =>
        "A detailed lucky cat hanging charm.";

    public override string Category =>
        "Luck";

    public override string Collection =>
        "Lucky Charms";

    public override bool IsPremium =>
        true;

    public override bool IsSeasonal =>
        false;

    protected override double ImageWidth => 145;

    protected override double ImageHeight => 160;   

    // IMPORTANT: preserve the PNG's proportions
    protected override Stretch ImageStretch =>
        Stretch.Uniform;

    // Make the cat visually wider without squashing it vertically
    protected override double ImageScaleX =>
        1.10;

    public override double HangPointX =>
        ImageWidth / 2;

    protected override string AssetPath =>
        "Assets/Dangles/maneki_neko.png";
}