using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LuckyDangle.Dangles;

using LuckyDangle.Services;
using System.Windows.Media.Imaging;

namespace LuckyDangle;

public partial class CharmPickerWindow : Window
{
    // =========================================================
    // PREMIUM THEME
    // =========================================================

    private const string Navy =
        "#17243A";

    private const string NavyLight =
        "#243149";

    private const string NavySoft =
        "#31415A";

    private const string Ivory =
        "#FFF9EC";

    private const string IvoryBright =
        "#FFFDF7";

    private const string Champagne =
        "#E6D4A7";

    private const string Gold =
        "#C9A34A";

    private const string GoldBright =
        "#D9B65C";

    private const string GoldDark =
        "#8B681E";

    private const string GoldSoft =
        "#F5E7C2";

    private const string TextDark =
        "#17243A";

    private const string TextMuted =
        "#6E7B8F";

    private readonly IDangle currentDangle;

    private IDangle? selectedDangle;

    private bool showingCollectionDangles;

    private string? currentCollection;


    public IDangle SelectedDangle =>
        selectedDangle ?? currentDangle;


    // =========================================================
    // CONSTRUCTOR
    // =========================================================

    public CharmPickerWindow(IDangle currentDangle)
    {
        InitializeComponent();

        RefreshPremiumAccessBadge();

        this.currentDangle =
            currentDangle;

        selectedDangle =
            currentDangle;

        CollectionHeader.Visibility =
            Visibility.Collapsed;

        BuildFilters();

        ShowCollections(
            DangleCatalog.GetAll());
    }


    // =========================================================
    // FILTER BAR
    // =========================================================

    private void BuildFilters()
    {
        FilterPanel.Children.Clear();

        AddFilterButton(
            "✦",
            "All",
            () => ShowCollections(
                DangleCatalog.GetAll()),
            true);

        AddFilterButton(
            "◇",
            "Free",
            () => ShowCollections(
                DangleCatalog.GetFree()));

        AddFilterButton(
            "✦",
            "Premium",
            () => ShowCollections(
                DangleCatalog.GetPremium()));

        AddFilterButton(
            "❄",
            "Seasonal",
            () => ShowCollections(
                DangleCatalog.GetSeasonal()));

        AddFilterButton(
            "🎁",
            "Festive",
            () => ShowCollections(
                DangleCatalog.GetCategory(
                    "Festive")));

        AddFilterButton(
            "🍀",
            "Luck",
            () => ShowCollections(
                DangleCatalog.GetCategory(
                    "Luck")));
    }


    private void AddFilterButton(
    string icon,
    string text,
    Action action,
    bool selected = false)
    {
        bool isPremium =
        text == "Premium";


        var button =
            new Button
            {
                Tag = text,
                Content =
        new StackPanel
        {
            Orientation =
                Orientation.Horizontal,

            HorizontalAlignment =
                HorizontalAlignment.Center,

            VerticalAlignment =
                VerticalAlignment.Center,

            Children =
            {
            new TextBlock
{
    Text = icon,

    FontSize = 14,

    Foreground =
        text switch
        {
            "All" => "#FFFFFF".ToBrush(),
            "Free" => "#BFD9F5".ToBrush(),
            "Premium" => "#FFD866".ToBrush(),
            "Seasonal" => "#A9D8FF".ToBrush(),
            "Festive" => "#FFD166".ToBrush(),
            "Luck" => "#8FD694".ToBrush(),
            _ => "#FFFFFF".ToBrush()
        },

    Margin =
        new Thickness(
            0,
            0,
            7,
            0),

    VerticalAlignment =
        VerticalAlignment.Center
},

            new TextBlock
            {
                Text = text,

                FontSize = 12.5,

                VerticalAlignment =
                    VerticalAlignment.Center
            }
            }
        },

                Height = 40,

                Padding =
                    new Thickness(
                        17,
                        0,
                        17,
                        0),

                Margin =
                    new Thickness(
                        0,
                        0,
                        6,
                        0),

                FontSize = 12.5,

                FontWeight =
                    selected
                        ? FontWeights.SemiBold
                        : FontWeights.Normal,

                Cursor =
                    System.Windows.Input.Cursors.Hand,

                Background =
                    selected
                        ? IvoryBright.ToBrush()
                        : isPremium
                            ? "#3A3020".ToBrush()
                            : NavyLight.ToBrush(),

                Foreground =
                    selected
                        ? TextDark.ToBrush()
                        : isPremium
                            ? GoldBright.ToBrush()
                            : "#C2CBD8".ToBrush(),

                BorderBrush =
                    selected
                        ? Gold.ToBrush()
                        : isPremium
                            ? "#8E6B2D".ToBrush()
                            : Brushes.Transparent,

                BorderThickness =
                    new Thickness(1)
            };


        button.Template =
                CreateFilterButtonTemplate();


        button.Click += (_, _) =>
            {
                action();


                // -------------------------------------------------
                // RESET FILTERS
                // -------------------------------------------------

                foreach (var child
                         in FilterPanel.Children)
                {
                    if (child is not Button filter)
                        continue;


                    bool premiumFilter =
    filter.Tag?.ToString() ==
    "Premium";


                    filter.Background =
                                premiumFilter
                                    ? "#3A3020".ToBrush()
                                    : NavyLight.ToBrush();


                    filter.Foreground =
                        premiumFilter
                            ? GoldBright.ToBrush()
                            : "#C2CBD8".ToBrush();


                    filter.BorderBrush =
                                premiumFilter
                                    ? "#8E6B2D".ToBrush()
                                    : Brushes.Transparent;


                    filter.FontWeight =
                        FontWeights.Normal;
                }


                // -------------------------------------------------
                // SELECTED FILTER
                // -------------------------------------------------

                button.Background = IvoryBright.ToBrush();

                button.Foreground =
                    TextDark.ToBrush();

                button.BorderBrush =
                    Gold.ToBrush();

                button.FontWeight =
                    FontWeights.SemiBold;
            };


        FilterPanel.Children.Add(button);
    }


    // =========================================================
    // FILTER BUTTON TEMPLATE
    // =========================================================

    private static ControlTemplate
        CreateFilterButtonTemplate()
    {
        var template =
            new ControlTemplate(
                typeof(Button));


        var border =
            new FrameworkElementFactory(
                typeof(Border));


        border.SetValue(
            Border.CornerRadiusProperty,
            new CornerRadius(20));


        border.SetBinding(
            Border.PaddingProperty,
            new System.Windows.Data.Binding(
                "Padding")
            {
                RelativeSource =
                    new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode
                            .TemplatedParent)
            });


        border.SetBinding(
            Border.BackgroundProperty,
            new System.Windows.Data.Binding(
                "Background")
            {
                RelativeSource =
                    new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode
                            .TemplatedParent)
            });


        border.SetBinding(
            Border.BorderBrushProperty,
            new System.Windows.Data.Binding(
                "BorderBrush")
            {
                RelativeSource =
                    new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode
                            .TemplatedParent)
            });


        border.SetBinding(
            Border.BorderThicknessProperty,
            new System.Windows.Data.Binding(
                "BorderThickness")
            {
                RelativeSource =
                    new System.Windows.Data.RelativeSource(
                        System.Windows.Data.RelativeSourceMode
                            .TemplatedParent)
            });


        var content =
            new FrameworkElementFactory(
                typeof(ContentPresenter));


        content.SetValue(
            ContentPresenter.HorizontalAlignmentProperty,
            HorizontalAlignment.Center);


        content.SetValue(
            ContentPresenter.VerticalAlignmentProperty,
            VerticalAlignment.Center);


        border.AppendChild(content);

        template.VisualTree = border;

        return template;
    }


    // =========================================================
    // COLLECTION VIEW
    // =========================================================

    private void ShowCollections(
        IReadOnlyList<IDangle> dangles)
    {
        showingCollectionDangles = false;

        currentCollection = null;

        CollectionHeader.Visibility =
    Visibility.Collapsed;

        DanglePanel.Children.Clear();

        BackPanel.Children.Clear();


        var collections =
            dangles
                .GroupBy(
                    d => d.Collection)
                .OrderBy(
                    g => g.Key)
                .ToList();


        foreach (var collection in collections)
        {
            DanglePanel.Children.Add(
                CreateCollectionCard(
                    collection.Key,
                    collection.ToList()));
        }


        CountText.Text =
            collections.Count == 1
                ? "1 collection"
                : $"{collections.Count} collections";
    }


    // =========================================================
    // COLLECTION CARD
    // =========================================================

    private Border CreateCollectionCard(
        string collectionName,
        IReadOnlyList<IDangle> dangles)
    {
        var card =
            new Border
            {
                Width = 165,
                Height = 300,
                Margin = new Thickness(6),

                Padding =
                    new Thickness(
                        11),

                Background =
                    IvoryBright.ToBrush(),

                BorderBrush =
                    Gold.ToBrush(),

                BorderThickness =
                    new Thickness(
                        1.4),

                CornerRadius =
                    new CornerRadius(
                        18),

                Cursor =
                    System.Windows.Input.Cursors.Hand
            };


        var stack =
            new StackPanel();


        // =====================================================
        // PREVIEW
        // =====================================================

        var preview =
            new Border
            {
                Height = 215,

                Background =
                    Champagne.ToBrush(),

                CornerRadius =
                    new CornerRadius(
                        13),

                Margin =
                    new Thickness(
                        0,
                        0,
                        0,
                        10)
            };


        var previewCanvas =
    new Canvas
    {
        Width = 120,
        Height = 215
    };


        // -----------------------------------------------------
        // Render first design
        // -----------------------------------------------------

        var previewDangle =
            dangles[0];

        previewDangle.Render(
            previewCanvas);


        // -----------------------------------------------------
        // CENTER BY HANG POINT
        // -----------------------------------------------------
        //
        // The artwork's HangPointX should sit exactly at
        // the horizontal center of the collection preview.
        //

        double artworkOffsetX =
            (previewCanvas.Width / 2.0)
            - previewDangle.HangPointX;

        Canvas.SetLeft(
            previewCanvas,
            artworkOffsetX);

        Canvas.SetTop(
            previewCanvas,
            0);


        // -----------------------------------------------------
        // HOST CANVAS
        // -----------------------------------------------------

        var previewHost =
            new Canvas
            {
                Width = 120,
                Height = 215
            };

        previewHost.Children.Add(
            previewCanvas);


        // -----------------------------------------------------
        // VIEWBOX
        // -----------------------------------------------------

        var viewBox =
            new Viewbox
            {
                Stretch =
                    Stretch.Uniform,

                StretchDirection =
                    StretchDirection.DownOnly,

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                VerticalAlignment =
                    VerticalAlignment.Center,

                Child =
                    previewHost
            };


        preview.Child =
            viewBox;


        // =====================================================
        // COLLECTION NAME
        // =====================================================

        var name =
            new TextBlock
            {
                Text =
                    collectionName,

                FontSize =
                    16,

                FontWeight =
                    FontWeights.SemiBold,

                Foreground =
                    TextDark.ToBrush(),

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                TextAlignment =
                    TextAlignment.Center
            };


        // =====================================================
        // DESIGN COUNT
        // =====================================================

        var count =
            new TextBlock
            {
                Text =
                    dangles.Count == 1
                        ? "1 design"
                        : $"{dangles.Count} designs",

                FontSize =
                    11,

                Foreground =
                    GoldDark.ToBrush(),

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                Margin =
                    new Thickness(
                        0,
                        4,
                        0,
                        0)
            };


        stack.Children.Add(
            preview);

        stack.Children.Add(
            name);

        stack.Children.Add(
            count);


        card.Child =
            stack;


        // =====================================================
        // OPEN COLLECTION
        // =====================================================

        card.MouseLeftButtonUp +=
            (_, _) =>
            {
                ShowCollectionDangles(
                    collectionName,
                    dangles);
            };


        return card;
    }


    // =========================================================
    // COLLECTION → DANGLERS
    // =========================================================

    private void ShowCollectionDangles(
     string collectionName,
     IReadOnlyList<IDangle> dangles)
    {
        showingCollectionDangles = true;

        currentCollection =
            collectionName;

        // -----------------------------------------
        // PREMIUM COLLECTION HEADER
        // -----------------------------------------

        CollectionHeader.Visibility =
            Visibility.Visible;

        CollectionTitle.Text =
            $"{collectionName} Collection";

        CollectionCount.Text =
            dangles.Count == 1
                ? "1 design"
                : $"{dangles.Count} designs";


        var (icon, subtitle) =
            GetCollectionPresentation(
                collectionName);

        CollectionIcon.Text =
            icon;

        CollectionSubtitle.Text =
            subtitle;




        // -----------------------------------------
        // SHOW DESIGNS
        // -----------------------------------------

        ShowDangles(
            dangles);

        AddBackToCollectionsButton();
    }


    // =========================================================
    // BACK TO COLLECTIONS
    // =========================================================

    private void AddBackToCollectionsButton()
    {
        BackPanel.Children.Clear();


        var backButton =
            new Button
            {
                Content =
                    "←  Collections",

                Height =
                    34,

                Padding =
                    new Thickness(
                        15,
                        0,
                        15,
                        0),

                FontSize =
                    12.5,

                FontWeight =
                    FontWeights.SemiBold,

                Foreground =
                    "#D9B65C".ToBrush(),

                Background =
                    NavyLight.ToBrush(),

                BorderBrush =
                    "#8E6B2D".ToBrush(),

                BorderThickness =
                    new Thickness(1),

                HorizontalAlignment =
                    HorizontalAlignment.Left,

                Cursor =
                    System.Windows.Input.Cursors.Hand
            };


        backButton.Template =
            CreateFilterButtonTemplate();


        backButton.Click +=
            (_, _) =>
            {
                showingCollectionDangles =
                    false;

                currentCollection =
                    null;

                ShowCollections(
                    DangleCatalog.GetAll());
            };


        BackPanel.Children.Add(
            backButton);
    }

    private static (
    string Icon,
    string Subtitle)
    GetCollectionPresentation(
        string collectionName)
    {
        return collectionName switch
        {
            "Rakhi" =>
                ("🌸",
                 "Love  •  Protection  •  Bond Forever"),

            "Lucky Charms" =>
                ("🍀",
                 "Fortune  •  Prosperity  •  Good Vibes"),

            "Festive" =>
                ("🎁",
                 "Celebrate  •  Joy  •  Togetherness"),

            "Seasonal" =>
                ("✨",
                 "Special  •  Limited  •  Memorable"),

            _ =>
                ("✦",
                 "Special designs  •  Made for your desktop")
        };
    }

    // =========================================================
    // DANGLER VIEW
    // =========================================================

    private void ShowDangles(
    IReadOnlyList<IDangle> dangles)
    {
        DanglePanel.Children.Clear();

        foreach (var dangle in dangles)
        {
            DanglePanel.Children.Add(
                CreateDangleCard(dangle));
        }

        CountText.Text =
            dangles.Count == 1
                ? "1 dangle"
                : $"{dangles.Count} dangles";
    }


    // =========================================================
    // DANGLER CARD
    // =========================================================

    private Border CreateDangleCard(
    IDangle dangle)
    {
        bool premium =
            dangle.IsPremium;

        bool locked =
            dangle.RequiresPremiumAccess &&
            !DangleAccessService.CanUse(dangle);


        // =====================================================
        // CARD
        // =====================================================

        var card =
            new Border
            {
                Width = 145,

                Height = 300,

                Margin =
                    new Thickness(4),

                Padding =
                    new Thickness(6),

                Background =
                    IvoryBright.ToBrush(),

                BorderBrush =
                    premium
                        ? Gold.ToBrush()
                        : "#D9CFAE".ToBrush(),

                BorderThickness =
                    new Thickness(
                        premium ? 1.4 : 1),

                CornerRadius =
                    new CornerRadius(15),

                Tag = dangle,

                Cursor =
                    System.Windows.Input.Cursors.Hand
            };


        var root =
            new Grid();


        // =====================================================
        // MAIN CONTENT
        // =====================================================

        var stack =
            new StackPanel();


        // =====================================================
        // ARTWORK
        // =====================================================

        var preview =
            new Border
            {
                Height = 215,

                Background =
                    Champagne.ToBrush(),

                CornerRadius =
                    new CornerRadius(11),

                Margin =
                    new Thickness(
                        0,
                        0,
                        0,
                        7)
            };


        var previewCanvas =
    new Canvas
    {
        Width = 120,
        Height = 215
    };


        // Render the dangle
        dangle.Render(
            previewCanvas);


        // Center the artwork using its HangPointX.
        // The HangPoint must sit at the center of the preview.
        double artworkOffsetX =
            (previewCanvas.Width / 2.0)
            - dangle.HangPointX;

        Canvas.SetLeft(
            previewCanvas,
            artworkOffsetX);

        Canvas.SetTop(
            previewCanvas,
            0);


        // Host canvas
        var previewHost =
            new Canvas
            {
                Width = 120,
                Height = 215
            };

        previewHost.Children.Add(
            previewCanvas);


        // Scale the complete preview
        var viewBox =
            new Viewbox
            {
                Stretch =
                    Stretch.Uniform,

                StretchDirection =
                    StretchDirection.DownOnly,

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                VerticalAlignment =
                    VerticalAlignment.Center,

                Child =
                    previewHost
            };

        preview.Child =
            viewBox;


        // =====================================================
        // NAME
        // =====================================================

        var name =
            new TextBlock
            {
                Text =
                    dangle.Name,

                FontFamily =
                    new FontFamily("Georgia"),

                FontSize =
                    12.5,

                FontWeight =
                    FontWeights.Bold,

                Foreground =
                    TextDark.ToBrush(),

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                TextAlignment =
                    TextAlignment.Center,

                TextWrapping =
                    TextWrapping.Wrap,

                MaxHeight = 34
            };


        // =====================================================
        // BADGES
        // =====================================================

        var badges =
            new StackPanel
            {
                Orientation =
                    Orientation.Horizontal,

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                Margin =
                    new Thickness(
                        0,
                        5,
                        0,
                        3)
            };


        badges.Children.Add(
            CreateBadge(
                locked
                    ? "🔒 Premium"
                    : premium
                        ? "✦ Premium"
                        : "Free",
                premium
                    ? "#F3C75B"
                    : "#9FD4FF",
                premium
                    ? "#5B4100"
                    : "#064B91"));


        if (dangle.IsSeasonal)
        {
            badges.Children.Add(
                CreateBadge(
                    "Seasonal",
                    "#FFE0C2",
                    "#A84516"));
        }


        // =====================================================
        // DESCRIPTION
        // =====================================================

        var description =
            new TextBlock
            {
                Text =
                    ShortDescription(
                        dangle.Description),

                FontSize =
                    9.5,

                Foreground =
                    "#5C6C82".ToBrush(),

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                TextAlignment =
                    TextAlignment.Center,

                TextWrapping =
                    TextWrapping.Wrap,

                MaxHeight = 40
            };


        stack.Children.Add(
            preview);

        stack.Children.Add(
            name);

        stack.Children.Add(
            badges);

        stack.Children.Add(
            description);


        root.Children.Add(
            stack);


        // =====================================================
        // SELECTED CHECK
        // =====================================================

        var check =
            new Border
            {
                Width = 28,

                Height = 28,

                CornerRadius =
                    new CornerRadius(14),

                Background =
                    "#29200B".ToBrush(),

                HorizontalAlignment =
                    HorizontalAlignment.Right,

                VerticalAlignment =
                    VerticalAlignment.Top,

                Margin =
                    new Thickness(
                        0,
                        4,
                        4,
                        0),

                Visibility =
                    dangle == selectedDangle
                        ? Visibility.Visible
                        : Visibility.Collapsed
            };


        check.Child =
            new TextBlock
            {
                Text = "✓",

                FontSize = 17,

                FontWeight =
                    FontWeights.Bold,

                Foreground =
                    "#FFD866".ToBrush(),

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                VerticalAlignment =
                    VerticalAlignment.Center
            };


        root.Children.Add(
            check);


        card.Child =
            root;


        // =====================================================
        // CLICK
        // =====================================================

        card.MouseLeftButtonUp +=
            (_, _) =>
            {
                SelectDangle(
                    dangle,
                    card);
            };


        return card;
    }

    private static Border CreateBadge(
    string text,
    string background,
    string foreground)
    {
        return new Border
        {
            Background =
                background.ToBrush(),

            CornerRadius =
                new CornerRadius(10),

            Padding =
                new Thickness(
                    7,
                    3,
                    7,
                    3),

            Margin =
                new Thickness(
                    2,
                    0,
                    2,
                    0),

            Child =
                new TextBlock
                {
                    Text = text,

                    FontSize = 8.5,

                    FontWeight =
                        FontWeights.SemiBold,

                    Foreground =
                        foreground.ToBrush(),

                    VerticalAlignment =
                        VerticalAlignment.Center
                }
        };
    }

    private static string ShortDescription(
    string description)
    {
        if (string.IsNullOrWhiteSpace(
                description))
            return "";

        if (description.Length <= 30)
            return description;

        return description[..30] + "…";
    }


    // =========================================================
    // SELECT DANGLER
    // =========================================================

    private void SelectDangle(
    IDangle dangle,
    Border selectedCard)
    {
        if (!DangleAccessService.CanUse(dangle))
        {
            new PremiumUpsellWindow { Owner = this }.ShowDialog();

            return;
        }

        selectedDangle =
            dangle;


        foreach (var child
                 in DanglePanel.Children)
        {
            if (child is not Border card)
                continue;


            card.Background =
                IvoryBright.ToBrush();

            if (card.Tag is IDangle cardDangle)
            {
                card.BorderBrush =
                    cardDangle.IsPremium
                        ? Gold.ToBrush()
                        : "#D9CFAE".ToBrush();

                card.BorderThickness =
                    new Thickness(
                        cardDangle.IsPremium
                            ? 1.4
                            : 1);
            }


            if (card.Child is Grid grid)
            {
                foreach (var element
                         in grid.Children)
                {
                    if (element is Border check
                        && check.Child is TextBlock)
                    {
                        check.Visibility =
                            Visibility.Collapsed;
                    }
                }
            }
        }


        selectedCard.Background =
            GoldSoft.ToBrush();

        selectedCard.BorderBrush =
            GoldBright.ToBrush();

        selectedCard.BorderThickness =
            new Thickness(2);


        if (selectedCard.Child is Grid selectedGrid)
        {
            foreach (var element
                     in selectedGrid.Children)
            {
                if (element is Border check
                    && check.Child is TextBlock)
                {
                    check.Visibility =
                        Visibility.Visible;
                }
            }
        }
    }


    // =========================================================
    // META TEXT
    // =========================================================

    private static string BuildMetaText(
        IDangle dangle)
    {
        var parts =
            new List<string>();


        if (dangle.IsPremium)
            parts.Add(
                "✦ Premium");
        else
            parts.Add(
                "Free");


        if (dangle.IsSeasonal)
            parts.Add(
                "Seasonal");


        return string.Join(
            "  •  ",
            parts);
    }

    // =========================================================
    // Support
    // =========================================================

    private void BuyCoffeeButton_Click(
    object sender,
    RoutedEventArgs e)
    {
        var window =
            new UI.Support.SupportWindow
            {
                Owner = this
            };

        window.Show();
    }


    // =========================================================
    // APPLY
    // =========================================================

    private void ApplyButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (selectedDangle == null)
            selectedDangle =
                currentDangle;

        if (!DangleAccessService.CanUse(selectedDangle))
        {
            MessageBox.Show(
                this,
                "This dangle requires an active Premium entitlement.",
                "Lucky Dangle Premium",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        DialogResult =
            true;
    }


    // =========================================================
    // CLOSE
    // =========================================================

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        DialogResult =
            false;
    }


    // =========================================================
    // BRUSH HELPER
    // =========================================================

    private void PremiumAccessButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        var premium = new PremiumUpsellWindow
        {
            Owner = this
        };

        premium.ShowDialog();
        RefreshPremiumAccessBadge();
    }

    private void RefreshPremiumAccessBadge()
    {
        var entitlement = PremiumEntitlementStore.Load();

        PremiumAccessButton.Tag =
            entitlement is not null &&
            entitlement.ExpiresAtUtc > DateTime.UtcNow
                ? "ACTIVE"
                : "UNLOCK";
    }
    private static SolidColorBrush Brush(
        string hex)
    {
        return new SolidColorBrush(
            (Color)ColorConverter
                .ConvertFromString(
                    hex)!);
    }
}


// =============================================================
// STRING → BRUSH HELPER
// =============================================================

internal static class BrushExtensions
{
    public static SolidColorBrush ToBrush(
        this string hex)
    {
        return new SolidColorBrush(
            (Color)ColorConverter
                .ConvertFromString(
                    hex)!);
    }
}