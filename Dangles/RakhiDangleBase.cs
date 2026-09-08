using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LuckyDangle.Dangles;

public abstract class RakhiDangleBase : IDangle
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Category { get; }
    public abstract string Collection { get; }
    public abstract bool IsPremium { get; }

    // Existing Rakhi dangles remain usable unless a future paid-only
    // Rakhi explicitly overrides this property.
    public virtual bool RequiresPremiumAccess => false;

    public abstract bool IsSeasonal { get; }

    public virtual double HangPointX => 45;
    public abstract void Render(Canvas canvas);

    protected static void Clear(Canvas canvas) => canvas.Children.Clear();

    protected static SolidColorBrush Brush(string hex) =>
        new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!);

    protected static Ellipse AddEllipse(Canvas canvas, double left, double top,
        double width, double height, string fill, string? stroke = null, double thickness = 1)
    {
        var ellipse = new Ellipse { Width = width, Height = height, Fill = Brush(fill) };
        if (stroke != null)
        {
            ellipse.Stroke = Brush(stroke);
            ellipse.StrokeThickness = thickness;
        }
        Canvas.SetLeft(ellipse, left);
        Canvas.SetTop(ellipse, top);
        canvas.Children.Add(ellipse);
        return ellipse;
    }

    protected static Line AddLine(Canvas canvas, double x1, double y1,
        double x2, double y2, string stroke, double thickness)
    {
        var line = new Line
        {
            X1 = x1, Y1 = y1, X2 = x2, Y2 = y2,
            Stroke = Brush(stroke),
            StrokeThickness = thickness,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        canvas.Children.Add(line);
        return line;
    }

    protected static Polygon AddPolygon(Canvas canvas, PointCollection points,
        string fill, string? stroke = null, double thickness = 1)
    {
        var polygon = new Polygon { Points = points, Fill = Brush(fill) };
        if (stroke != null)
        {
            polygon.Stroke = Brush(stroke);
            polygon.StrokeThickness = thickness;
        }
        canvas.Children.Add(polygon);
        return polygon;
    }

    protected static TextBlock AddText(Canvas canvas, string text,
        double left, double top, double width, double height, double fontSize,
        string foreground, FontWeight? weight = null, string? fontFamily = null)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            Width = width,
            Height = height,
            FontSize = fontSize,
            FontWeight = weight ?? FontWeights.Bold,
            Foreground = Brush(foreground),
            TextAlignment = TextAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = fontFamily != null ? new FontFamily(fontFamily) : new FontFamily("Segoe UI")
        };
        Canvas.SetLeft(textBlock, left);
        Canvas.SetTop(textBlock, top);
        canvas.Children.Add(textBlock);
        return textBlock;
    }

    protected static void AddGoldConnector(Canvas canvas, double x = 45)
    {
        AddLine(canvas, x, 0, x, 8, "#C99620", 3);
        AddEllipse(canvas, x - 6, 5, 12, 12, "#F4C94F", "#8B6417", 1);
    }

    protected static void AddSmallJewel(Canvas canvas, double x, double y,
        string fill, double size = 8)
    {
        AddEllipse(canvas, x, y, size, size, fill, "#FFF0A8", 0.7);
    }
}
