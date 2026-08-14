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

    // Commercial status of THIS design.
    bool IsPremium { get; }

    // Useful for things such as Rakhi, Diwali, Christmas etc.
    bool IsSeasonal { get; }

    double HangPointX { get; }

    // Draw the actual dangle.
    void Render(Canvas canvas);
}