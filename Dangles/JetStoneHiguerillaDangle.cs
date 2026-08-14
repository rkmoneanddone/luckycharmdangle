namespace LuckyDangle.Dangles;

public sealed class JetStoneHiguerillaDangle : ImageDangleBase
{
    public override string Id =>
        "jet_stone_higuerilla";

    public override string Name =>
        "Jet Stone / Higuerilla";

    public override string Description =>
        "A Latin American protection talisman featuring polished black higuerilla seeds, red cord, gold accents, Nazar eyes and traditional good-luck symbols.";

    public override string Category =>
    "Protection";

    public override string Collection =>
        "Lucky Charms";

    public override bool IsPremium =>
        true;

    public override bool IsSeasonal =>
        false;

    protected override string AssetPath =>
        "Assets/Dangles/jet_stone_higuerilla.png";

    protected override double ImageWidth =>
        145;

    protected override double ImageHeight =>
        220;

    protected override double ImageScaleX =>
        1.0;

    public override double HangPointX =>
        ImageWidth / 2;
}