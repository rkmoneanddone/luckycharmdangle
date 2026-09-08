using System;
using System.IO;
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

    // Backward compatibility: every previously shipped dangle stays usable
    // unless a NEW dangle class explicitly overrides this property.
    public virtual bool RequiresPremiumAccess => false;

    public abstract bool IsSeasonal { get; }

    protected abstract string AssetPath { get; }

    public string ShareAssetPath => AssetPath;

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

        string fullPath =
            Path.Combine(
                AppContext.BaseDirectory,
                AssetPath.Replace(
                    '/',
                    Path.DirectorySeparatorChar));

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
                    0.5)
        };

        var bitmap = new BitmapImage();

        bitmap.BeginInit();

        bitmap.UriSource =
            new Uri(
                fullPath,
                UriKind.Absolute);

        bitmap.CacheOption =
            BitmapCacheOption.OnLoad;

        bitmap.EndInit();

        bitmap.Freeze();

        image.Source = bitmap;

        Canvas.SetLeft(
            image,
            ImageLeft);

        Canvas.SetTop(
            image,
            ImageTop);

        canvas.Children.Add(image);
    }
}