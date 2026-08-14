using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LuckyDangle.Dangles;

public class LuckyCoinDangle : IDangle
{
    public string Id =>
    "lucky_coin_classic";

    public string Name =>
        "Classic Lucky Coin";

    public string Description =>
        "A traditional prosperity charm.";

    public string Category =>
        "Luck";

    public string Collection =>
        "Lucky Charms";

    public bool IsPremium =>
        false;

    public bool IsSeasonal =>
        false;

public double HangPointX => 45;
    public void Render(Canvas canvas)
    {
        canvas.Children.Clear();

        // =====================================================
        // TOP CONNECTOR
        // =====================================================

        AddLine(
            canvas,
            45, 0,
            45, 7,
            "#B88922",
            3);


        // =====================================================
        // GOLD BEAD
        // =====================================================

        AddEllipse(
            canvas,
            39, 4,
            12, 12,
            "#E8B93E",
            "#8B6415",
            1);


        // =====================================================
        // SMALL RED BEAD
        // =====================================================

        AddEllipse(
            canvas,
            40, 14,
            10, 10,
            "#B51F2D",
            "#74121B",
            1);


        // =====================================================
        // COIN OUTER RIM
        // =====================================================

        AddEllipse(
            canvas,
            10, 25,
            70, 70,
            "#A96F08",
            "#694508",
            2);


        // =====================================================
        // OUTER GOLD HIGHLIGHT
        // =====================================================

        AddEllipse(
            canvas,
            13, 28,
            64, 64,
            "#F0C54A",
            "#D69B20",
            1);


        // =====================================================
        // RED ENAMEL FACE
        // =====================================================

        AddEllipse(
            canvas,
            18, 33,
            54, 54,
            "#B51F2D",
            "#7D141D",
            2);


        // =====================================================
        // INNER GOLD RING
        // =====================================================

        AddEllipse(
            canvas,
            23, 38,
            44, 44,
            "#E7B93D",
            "#8A6215",
            1);


        // =====================================================
        // INNER RED FIELD
        // =====================================================

        AddEllipse(
            canvas,
            27, 42,
            36, 36,
            "#C52835",
            "#8A1721",
            1);


        // =====================================================
        // PROSPERITY CHARACTER
        // =====================================================

        AddText(
            canvas,
            "福",
            30,
            43,
            26,
            "#FFD967");


        // =====================================================
        // SMALL HIGHLIGHT
        // =====================================================

        AddEllipse(
            canvas,
            23, 38,
            7, 7,
            "#FFF0A8",
            null,
            0);


        // =====================================================
        // HANGING RED CORD
        // =====================================================

        AddLine(
            canvas,
            45, 94,
            45, 108,
            "#B51F2D",
            4);


        // =====================================================
        // BOTTOM GOLD BEAD
        // =====================================================

        AddEllipse(
            canvas,
            39, 102,
            12, 12,
            "#E8B93E",
            "#8B6415",
            1);


        // =====================================================
        // SMALL RED TASSEL
        // =====================================================

        AddLine(
            canvas,
            45, 112,
            45, 115,
            "#B51F2D",
            3);
    }


    // =========================================================
    // ELLIPSE
    // =========================================================

    private static void AddEllipse(
        Canvas canvas,
        double left,
        double top,
        double width,
        double height,
        string fill,
        string? stroke,
        double thickness)
    {
        var ellipse = new Ellipse
        {
            Width = width,
            Height = height,
            Fill = Brush(fill)
        };

        if (stroke != null)
        {
            ellipse.Stroke = Brush(stroke);
            ellipse.StrokeThickness = thickness;
        }

        Canvas.SetLeft(
            ellipse,
            left);

        Canvas.SetTop(
            ellipse,
            top);

        canvas.Children.Add(
            ellipse);
    }


    // =========================================================
    // LINE
    // =========================================================

    private static void AddLine(
        Canvas canvas,
        double x1,
        double y1,
        double x2,
        double y2,
        string stroke,
        double thickness)
    {
        var line = new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,

            Stroke = Brush(stroke),

            StrokeThickness =
                thickness,

            StrokeStartLineCap =
                PenLineCap.Round,

            StrokeEndLineCap =
                PenLineCap.Round
        };

        canvas.Children.Add(line);
    }


    // =========================================================
    // CHARACTER
    // =========================================================

    private static void AddText(
        Canvas canvas,
        string text,
        double left,
        double top,
        double fontSize,
        string foreground)
    {
        var textBlock = new TextBlock
        {
            Text = text,

            FontSize = fontSize,

            FontWeight =
                System.Windows.FontWeights.Bold,

            Foreground =
                Brush(foreground),

            Width = 30,

            Height = 32,

            TextAlignment =
                System.Windows.TextAlignment.Center
        };

        Canvas.SetLeft(
            textBlock,
            left);

        Canvas.SetTop(
            textBlock,
            top);

        canvas.Children.Add(
            textBlock);
    }


    // =========================================================
    // BRUSH
    // =========================================================

    private static SolidColorBrush Brush(
        string hex)
    {
        return new SolidColorBrush(
            (Color)ColorConverter
                .ConvertFromString(hex)!);
    }
}