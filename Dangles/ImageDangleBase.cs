using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LuckyDangle.Dangles;

public abstract class ImageDangleBase : IDangle
{
    public abstract string Id { get; }

    public abstract string Name { get; }

    public abstract string Description { get; }

    public abstract string Category { get; }

    public abstract string Collection { get; }

    public abstract bool IsPremium { get; }

    public abstract bool IsSeasonal { get; }

    protected abstract string AssetPath { get; }

    protected virtual double ImageWidth => 120;

    protected virtual double ImageHeight => 160;

    protected virtual double ImageLeft => 0;

    protected virtual double ImageTop => 0;
    protected virtual Stretch ImageStretch => Stretch.Uniform;

    protected virtual double ImageScaleX => 1.0;

    public virtual double HangPointX => ImageWidth / 2;
    public virtual void Render(Canvas canvas)
    {
        canvas.Children.Clear();

        var image = new Image
        {
            Width = ImageWidth,
            Height = ImageHeight,

            Stretch = ImageStretch,

            RenderTransform =
        new ScaleTransform(
            ImageScaleX,
            1.0),

            RenderTransformOrigin =
        new System.Windows.Point(
            0.5,
            0.5),

            Source = new BitmapImage(
        new Uri(
            AssetPath,
            UriKind.RelativeOrAbsolute))
        };

        Canvas.SetLeft(
    image,
    ImageLeft);

        Canvas.SetTop(
            image,
            ImageTop);

        canvas.Children.Add(image);
    }
}