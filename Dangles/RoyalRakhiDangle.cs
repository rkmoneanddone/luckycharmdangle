using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LuckyDangle.Dangles;

public class RoyalRakhiDangle : IDangle
{
    public string Id =>
        "rakhi_royal";

    public string Name =>
        "Royal Rakhi";

    public string Description =>
        "An ornate gold and crimson Rakhi.";

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
        // TOP HANGING THREAD
        // =====================================================

        AddLine(
            canvas,
            45, 0,
            45, 15,
            "#B88724",
            3);


        // =====================================================
        // GOLD TOP BEAD
        // =====================================================

        AddEllipse(
            canvas,
            39, 10,
            12, 12,
            "#E7B83D",
            "#8C6418",
            1);


        // =====================================================
        // CRIMSON SMALL BEAD
        // =====================================================

        AddEllipse(
            canvas,
            40, 20,
            10, 10,
            "#9D1727",
            "#67101A",
            1);


        // =====================================================
        // OUTER ORNAMENT
        // =====================================================

        AddEllipse(
            canvas,
            7, 27,
            76, 76,
            "#8E1425",
            "#5E0D18",
            2);


        // =====================================================
        // GOLD OUTER RIM
        // =====================================================

        AddEllipse(
            canvas,
            11, 31,
            68, 68,
            "#D6A32E",
            "#8C6418",
            2);


        // =====================================================
        // CRIMSON INNER RING
        // =====================================================

        AddEllipse(
            canvas,
            17, 37,
            56, 56,
            "#A7192B",
            "#72101D",
            2);


        // =====================================================
        // GOLD JEWEL RING
        // =====================================================

        AddEllipse(
            canvas,
            23, 43,
            44, 44,
            "#F0C54D",
            "#A87518",
            1.5);


        // =====================================================
        // INNER CRIMSON FACE
        // =====================================================

        AddEllipse(
            canvas,
            28, 48,
            34, 34,
            "#B7192E",
            "#7C111E",
            1);


        // =====================================================
        // CENTRE JEWEL
        // =====================================================

        AddEllipse(
            canvas,
            36, 56,
            18, 18,
            "#F7D76B",
            "#A87518",
            1);


        AddEllipse(
            canvas,
            40, 60,
            10, 10,
            "#FFF3B0",
            "#C08A20",
            1);


        // =====================================================
        // DECORATIVE GOLD PETALS
        // =====================================================

        AddEllipse(
            canvas,
            35, 37,
            8, 13,
            "#F1C64D",
            "#A87518",
            1);

        AddEllipse(
            canvas,
            47, 37,
            8, 13,
            "#F1C64D",
            "#A87518",
            1);

        AddEllipse(
            canvas,
            19, 58,
            8, 13,
            "#F1C64D",
            "#A87518",
            1);

        AddEllipse(
            canvas,
            61, 58,
            8, 13,
            "#F1C64D",
            "#A87518",
            1);


        // =====================================================
        // SIDE DECORATIVE BEADS
        // =====================================================

        AddEllipse(
            canvas,
            7, 58,
            8, 8,
            "#F0C54D",
            "#8C6418",
            1);

        AddEllipse(
            canvas,
            75, 58,
            8, 8,
            "#F0C54D",
            "#8C6418",
            1);

        AddEllipse(
            canvas,
            10, 78,
            8, 8,
            "#C53A55",
            "#74101D",
            1);

        AddEllipse(
            canvas,
            72, 78,
            8, 8,
            "#C53A55",
            "#74101D",
            1);


        // =====================================================
        // LOWER CORD
        // =====================================================

        AddLine(
            canvas,
            45, 103,
            45, 120,
            "#9D1727",
            4);


        // =====================================================
        // LOWER GOLD BEAD
        // =====================================================

        AddEllipse(
            canvas,
            39, 114,
            12, 12,
            "#E7B83D",
            "#8C6418",
            1);


        // =====================================================
        // TASSEL
        // =====================================================

        AddLine(
            canvas,
            39, 124,
            39, 132,
            "#9D1727",
            2);

        AddLine(
            canvas,
            45, 124,
            45, 134,
            "#9D1727",
            2);

        AddLine(
            canvas,
            51, 124,
            51, 132,
            "#9D1727",
            2);


        AddEllipse(
            canvas,
            37, 131,
            16, 7,
            "#8E1425",
            "#5E0D18",
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

        Canvas.SetLeft(ellipse, left);
        Canvas.SetTop(ellipse, top);

        canvas.Children.Add(ellipse);
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
            StrokeThickness = thickness,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
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