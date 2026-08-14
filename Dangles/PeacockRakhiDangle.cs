using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LuckyDangle.Dangles;

public class PeacockRakhiDangle : IDangle
{
    public string Id =>
        "rakhi_peacock";

    public string Name =>
        "Peacock Rakhi";

    public string Description =>
        "An ornate peacock-inspired Rakhi with jewel details.";

    public string Category =>
        "Festive";

    public string Collection =>
        "Rakhi";

    public bool IsPremium =>
        true;

    public bool IsSeasonal =>
        true;
    public double HangPointX => 45;

    public void Render(Canvas canvas)
    {
        canvas.Children.Clear();

        // =====================================================
        // TOP THREAD
        // =====================================================

        AddLine(
            canvas,
            45, 0,
            45, 13,
            "#B88724",
            3);


        // =====================================================
        // GOLD TOP BEAD
        // =====================================================

        AddEllipse(
            canvas,
            39, 9,
            12, 12,
            "#E8C34E",
            "#896116",
            1);


        // =====================================================
        // BLUE BEAD
        // =====================================================

        AddEllipse(
            canvas,
            40, 19,
            10, 10,
            "#168AA2",
            "#075B70",
            1);


        // =====================================================
        // PEACOCK FEATHER OUTER SHAPE
        // =====================================================

        AddEllipse(
            canvas,
            9, 27,
            72, 82,
            "#0B6578",
            "#064656",
            2);


        // =====================================================
        // GOLD OUTER ORNAMENT
        // =====================================================

        AddEllipse(
            canvas,
            13, 31,
            64, 74,
            "#D9AD32",
            "#8A6216",
            2);


        // =====================================================
        // DEEP TEAL INNER FIELD
        // =====================================================

        AddEllipse(
            canvas,
            18, 35,
            54, 66,
            "#087A88",
            "#075361",
            2);


        // =====================================================
        // PEACOCK FEATHER LAYERS
        // =====================================================

        AddEllipse(
            canvas,
            25, 40,
            40, 52,
            "#0B526E",
            "#073D53",
            1);


        AddEllipse(
            canvas,
            29, 44,
            32, 44,
            "#13A0A3",
            "#087174",
            1);


        // =====================================================
        // BLUE INNER EYE
        // =====================================================

        AddEllipse(
            canvas,
            34, 49,
            22, 30,
            "#1459A5",
            "#0B3C75",
            1);


        // =====================================================
        // GOLD EYE
        // =====================================================

        AddEllipse(
            canvas,
            37, 54,
            16, 22,
            "#E6BC3F",
            "#926816",
            1);


        // =====================================================
        // GREEN EYE
        // =====================================================

        AddEllipse(
            canvas,
            40, 57,
            10, 16,
            "#15945E",
            "#075B39",
            1);


        // =====================================================
        // TURQUOISE CENTER
        // =====================================================

        AddEllipse(
            canvas,
            42, 60,
            6, 10,
            "#63D4D0",
            "#167A78",
            1);


        // =====================================================
        // SMALL CENTER JEWEL
        // =====================================================

        AddEllipse(
            canvas,
            43, 63,
            4, 6,
            "#F6D76A",
            null,
            0);


        // =====================================================
        // GOLD SIDE ORNAMENTS
        // =====================================================

        AddEllipse(
            canvas,
            17, 53,
            8, 12,
            "#E7BD45",
            "#916716",
            1);

        AddEllipse(
            canvas,
            65, 53,
            8, 12,
            "#E7BD45",
            "#916716",
            1);


        AddEllipse(
            canvas,
            16, 72,
            7, 10,
            "#1599A2",
            "#075967",
            1);

        AddEllipse(
            canvas,
            67, 72,
            7, 10,
            "#1599A2",
            "#075967",
            1);


        // =====================================================
        // SMALL RED JEWELS
        // =====================================================

        AddEllipse(
            canvas,
            22, 88,
            7, 7,
            "#C62B42",
            "#75101F",
            1);

        AddEllipse(
            canvas,
            61, 88,
            7, 7,
            "#C62B42",
            "#75101F",
            1);


        // =====================================================
        // LOWER GOLD BEAD
        // =====================================================

        AddEllipse(
            canvas,
            39, 104,
            12, 12,
            "#E7BD45",
            "#8A6216",
            1);


        // =====================================================
        // BLUE HANGING THREAD
        // =====================================================

        AddLine(
            canvas,
            45, 114,
            45, 126,
            "#075C78",
            4);


        // =====================================================
        // DECORATIVE TASSEL
        // =====================================================

        AddLine(
            canvas,
            39, 124,
            38, 135,
            "#0A7186",
            2);

        AddLine(
            canvas,
            45, 124,
            45, 138,
            "#0A7186",
            2);

        AddLine(
            canvas,
            51, 124,
            52, 135,
            "#0A7186",
            2);


        // =====================================================
        // TASSEL GOLD CAP
        // =====================================================

        AddEllipse(
            canvas,
            38, 120,
            14, 9,
            "#D8AA30",
            "#866016",
            1);
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