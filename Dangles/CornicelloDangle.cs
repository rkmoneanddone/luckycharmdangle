namespace LuckyDangle.Dangles;

public sealed class CornicelloDangle : ImageDangleBase
{
    public override string Id =>
        "cornicello";

    public override string Name =>
        "Cornicello / Corno";

    public override string Description =>
        "An ornate Italian cornicello horn talisman with red enamel, gold filigree, Nazar eye and lucky charms.";

    public override string Category =>
    "Protection";

    public override string Collection =>
        "Lucky Charms";

    public override bool IsPremium =>
        true;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/cornicello.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        220;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}