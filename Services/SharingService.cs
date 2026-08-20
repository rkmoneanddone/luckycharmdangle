using System;
using System.Windows;
using LuckyDangle.Dangles;

namespace LuckyDangle.Services;

public static class SharingService
{
    public const string MicrosoftStoreUrl =
        "https://apps.microsoft.com/detail/9N11M525D0M9";

    public static void ShareDangle(
        IDangle dangle)
    {
        string message =
            BuildShareText(dangle);

        System.Windows.Clipboard.SetText(message);

        MessageBox.Show(
            "Share message copied!\n\n" +
            "You can now paste it into WhatsApp, Facebook, email, or anywhere else.",
            "Lucky Dangle",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private static string BuildShareText(
        IDangle dangle)
    {
        if (dangle.Collection.Equals(
            "Rakhi",
            StringComparison.OrdinalIgnoreCase))
        {
            return
                "❤️ अपने भाई, बहन या भाभी से शेयर करिए...\n" +
                "वो अपने laptop पर आपकी राखी पूरे साल रख सकेंगे।\n\n" +
                "Lucky Dangle: " + dangle.Name + "\n\n" +
                "Download Lucky Dangle:\n" +
                MicrosoftStoreUrl;
        }

        if (dangle.Collection.Equals(
            "Spiritual",
            StringComparison.OrdinalIgnoreCase))
        {
            return
                "🙏 अपनी पसंद का Spiritual Lucky Dangle अपने laptop पर रखिए।\n\n" +
                "Lucky Dangle: " + dangle.Name + "\n\n" +
                "Download Lucky Dangle:\n" +
                MicrosoftStoreUrl;
        }

        return
            "🍀 अपना Lucky Charm अपने laptop पर रखिए।\n\n" +
            "Lucky Dangle: " + dangle.Name + "\n\n" +
            "Download Lucky Dangle:\n" +
            MicrosoftStoreUrl;
    }
}