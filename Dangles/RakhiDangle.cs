using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LuckyDangle.Dangles;

public class RakhiDangle : IDangle
{
    public string Id =>
    "rakhi_classic";

    public string Name =>
        "Classic Rakhi";

    public string Description =>
        "A festive traditional Rakhi.";

    public string Category =>
        "Festive";

    public string Collection =>
        "Rakhi";

    public bool IsPremium =>
        false;

    public bool IsSeasonal =>
        true;

    public double HangPointX => 45;
    
    public void Render(Canvas canvas)
    {
        // Clear anything previously rendered
        canvas.Children.Clear();


        // =====================================================
        // TOP GOLD CONNECTOR
        // =====================================================

        AddLine(
            canvas,
            45, 0,
            45, 7,
            "#D3A43B",
            3);

        AddEllipse(
            canvas,
            38, 3,
            14, 14,
            "#F0C75E",
            "#8A641F",
            1);


        // =====================================================
        // OUTER RAKHI
        // =====================================================

        AddEllipse(
            canvas,
            15, 8,
            60, 60,
            "#FFF8FC",
            "#C69BC8",
            1.5);


        // =====================================================
        // PINK DECORATIVE RING
        // =====================================================

        AddEllipse(
            canvas,
            21, 14,
            48, 48,
            "#C83E7D",
            "#7D1F4E",
            2);


        // =====================================================
        // GOLD CENTRE
        // =====================================================

        AddEllipse(
            canvas,
            29, 22,
            32, 32,
            "#FFD76A",
            "#B98213",
            1.2);


        // =====================================================
        // DECORATIVE BEADS
        // =====================================================

        for (int i = 0; i < 8; i++)
        {
            double angle =
                i * Math.PI / 4;

            double x =
                41 + Math.Cos(angle) * 28;

            double y =
                31 + Math.Sin(angle) * 28;

            AddEllipse(
                canvas,
                x,
                y,
                8,
                8,
                i % 2 == 0
                    ? "#F3A8C8"
                    : "#6EC6C2",
                "#FFFFFF",
                0.6);
        }


        // =====================================================
        // CENTRE JEWEL
        // =====================================================

        AddEllipse(
            canvas,
            39, 29,
            12, 12,
            "#FFFFFF",
            "#C83E7D",
            1);

        AddEllipse(
            canvas,
            42, 32,
            6, 6,
            "#D5A22E",
            null,
            0);


        // =====================================================
        // HANGING THREAD
        // =====================================================

        AddLine(
            canvas,
            45, 68,
            45, 105,
            "#C83E7D",
            3);


        // =====================================================
        // LOWER DECORATIVE BEADS
        // =====================================================

        AddEllipse(
            canvas,
            34, 96,
            22, 12,
            "#C83E7D",
            "#7D1F4E",
            1);

        AddEllipse(
            canvas,
            39, 88,
            12, 12,
            "#FFD76A",
            "#B98213",
            1);
    }


    // =========================================================
    // DRAWING HELPERS
    // =========================================================

    private static Ellipse AddEllipse(
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

        return ellipse;
    }


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

        canvas.Children.Add(
            line);
    }


    private static SolidColorBrush Brush(
        string hex)
    {
        return new SolidColorBrush(
            (Color)ColorConverter
                .ConvertFromString(hex)!);
    }
}