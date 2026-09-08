using System.Windows.Controls;

namespace LuckyDangle.Dangles;

public interface IDangle
{
    // Unique identity of this specific design.
    // Example: "rakhi_classic"
    string Id { get; }

    // Display name of this specific design.
    // Example: "Classic Rakhi"
    string Name { get; }

    string Description { get; }

    // Broad category.
    // Example: "Festive", "Luck", "Spiritual"
    string Category { get; }

    // Collection / family.
    // Example: "Rakhi", "Lucky Charms", "Lotus"
    string Collection { get; }
    // Commercial / presentation status of THIS design.
    // Existing shipped dangles may be marked Premium but remain usable.
    bool IsPremium { get; }

    // Actual access-control flag.
    // Existing shipped dangles default to false via ImageDangleBase.
    // New paid-only dangles/collections explicitly override this to true.
    bool RequiresPremiumAccess { get; }

    // Useful for things such as Rakhi, Diwali, Christmas etc.
    bool IsSeasonal { get; }

    double HangPointX { get; }

    // Draw the actual dangle.
    void Render(Canvas canvas);
}