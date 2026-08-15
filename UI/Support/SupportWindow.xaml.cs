using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using QRCoder;

namespace LuckyDangle.UI.Support;

public partial class SupportWindow : Window
{
    private const string UpiId =
        "rohitmallick85@oksbi";

    private const string PayeeName =
        "Rohit Mallick";

    private const decimal MinimumAmount =
        100m;

    private decimal selectedAmount =
        100m;

    private bool updatingAmount;

    private const decimal MaximumAmount = 5000m;


    // =====================================================
    // CONSTRUCTOR
    // =====================================================

    public SupportWindow()
    {
        InitializeComponent();

        // The XAML already starts with Text="100".
        // InitializeComponent() can fire TextChanged before
        // PayButton exists, so UpdatePayButton() is null-safe.

        updatingAmount = true;

        CustomAmountTextBox.Text = "100";

        updatingAmount = false;

        selectedAmount = 100m;

        UpdatePayButton();
    }


    // =====================================================
    // PRESET AMOUNT
    // =====================================================

    private void PresetAmount_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.Tag is string value &&
            decimal.TryParse(
                value,
                out decimal amount))
        {
            updatingAmount = true;

            CustomAmountTextBox.Text =
                amount.ToString("0");

            CustomAmountTextBox.SelectAll();

            updatingAmount = false;

            selectedAmount =
                amount;

            UpdatePayButton();
        }
    }


    // =====================================================
    // ONLY ALLOW NUMBERS + DECIMAL POINT
    // =====================================================

    private void CustomAmountTextBox_PreviewTextInput(
    object sender,
    TextCompositionEventArgs e)
    {
        if (!Regex.IsMatch(e.Text, @"^[0-9.]$"))
        {
            e.Handled = true;
            return;
        }

        string currentText =
            CustomAmountTextBox.Text;

        int selectionStart =
            CustomAmountTextBox.SelectionStart;

        int selectionLength =
            CustomAmountTextBox.SelectionLength;

        string newText =
            currentText.Remove(
                selectionStart,
                selectionLength)
            .Insert(
                selectionStart,
                e.Text);

        // Don't allow more than 2 decimal places.
        int decimalIndex =
            newText.IndexOf('.');

        if (decimalIndex >= 0 &&
            newText.Length - decimalIndex - 1 > 2)
        {
            e.Handled = true;
            return;
        }

        // Don't allow more than 5 digits before decimal.
        string integerPart =
            decimalIndex >= 0
                ? newText[..decimalIndex]
                : newText;

        if (integerPart.Length > 5)
        {
            e.Handled = true;
            return;
        }

        // Don't allow more than ₹5,000.
        if (decimal.TryParse(
                newText,
                out decimal amount) &&
            amount > MaximumAmount)
        {
            e.Handled = true;
            return;
        }

        e.Handled = false;
    }

    private void CustomAmountTextBox_Pasting(
        object sender,
        DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(
                typeof(string)))
        {
            e.CancelCommand();
            return;
        }

        string pastedText =
            e.DataObject.GetData(
                typeof(string)) as string ?? "";

        string currentText =
            CustomAmountTextBox.Text;

        int selectionStart =
            CustomAmountTextBox.SelectionStart;

        int selectionLength =
            CustomAmountTextBox.SelectionLength;

        string newText =
            currentText.Remove(
                selectionStart,
                selectionLength)
            .Insert(
                selectionStart,
                pastedText);

        if (!Regex.IsMatch(
                newText,
                @"^\d{1,5}(\.\d{0,2})?$"))
        {
            e.CancelCommand();
            return;
        }

        if (!decimal.TryParse(
                newText,
                out decimal amount))
        {
            e.CancelCommand();
            return;
        }

        if (amount > MaximumAmount)
        {
            e.CancelCommand();
        }
    }


    // =====================================================
    // AMOUNT CHANGED
    // =====================================================

    private void CustomAmountTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (updatingAmount)
            return;

        UpdatePayButton();
    }


    // =====================================================
    // VALIDATE WHEN USER LEAVES THE FIELD
    // =====================================================

    private void CustomAmountTextBox_LostFocus(
    object sender,
    RoutedEventArgs e)
{
    if (!decimal.TryParse(
            CustomAmountTextBox.Text.Trim(),
            out decimal amount))
    {
        SetMinimumAmount();
        return;
    }

    if (amount < MinimumAmount)
    {
        SetMinimumAmount();
        return;
    }

    if (amount > MaximumAmount)
    {
        updatingAmount = true;

        CustomAmountTextBox.Text =
            MaximumAmount.ToString("0");

        updatingAmount = false;

        selectedAmount =
            MaximumAmount;

        UpdatePayButton();

        return;
    }

    selectedAmount =
        amount;

    UpdatePayButton();
}


    // =====================================================
    // UPDATE PAY BUTTON
    // =====================================================

    private void UpdatePayButton()
    {
        // IMPORTANT:
        // During InitializeComponent(), TextChanged can fire
        // before PayButton has been created.

        if (PayButton == null ||
            CustomAmountTextBox == null)
        {
            return;
        }


        if (decimal.TryParse(
                CustomAmountTextBox.Text.Trim(),
                out decimal amount) &&
            amount >= MinimumAmount)
        {
            PayButton.Content =
                $"Pay ₹{amount:0.##}";

            return;
        }


        PayButton.Content =
            "Pay";
    }


    // =====================================================
    // PAY
    // =====================================================

    private void Pay_Click(
    object sender,
    RoutedEventArgs e)
    {
        if (!decimal.TryParse(
                CustomAmountTextBox.Text.Trim(),
                out decimal amount))
        {
            SetMinimumAmount();
            return;
        }

        if (amount < MinimumAmount)
        {
            SetMinimumAmount();
            return;
        }

        if (amount > MaximumAmount)
        {
            CustomAmountTextBox.Text =
                MaximumAmount.ToString("0");

            UpdatePayButton();

            return;
        }

        amount =
            Math.Round(
                amount,
                2,
                MidpointRounding.AwayFromZero);

        selectedAmount =
            amount;

        GeneratePaymentQr(amount);

        PaymentAmountText.Text =
            $"₹{amount:0.##}";

        AmountScreen.Visibility =
            Visibility.Collapsed;

        PaymentScreen.Visibility =
            Visibility.Visible;
    }
    


    // =====================================================
    // SET MINIMUM AMOUNT
    // =====================================================

    private void SetMinimumAmount()
    {
        if (CustomAmountTextBox == null)
            return;


        updatingAmount = true;

        CustomAmountTextBox.Text =
            "100";

        updatingAmount = false;


        selectedAmount =
            MinimumAmount;


        UpdatePayButton();


        CustomAmountTextBox.Focus();

        CustomAmountTextBox.SelectAll();
    }


    // =====================================================
    // GENERATE AMOUNT-SPECIFIC UPI QR
    // =====================================================

    private void GeneratePaymentQr(
        decimal amount)
    {
        // Example:
        //
        // upi://pay
        // ?pa=rohitmallick85@oksbi
        // &pn=Rohit%20Mallick
        // &am=150.00
        // &cu=INR
        // &tn=Coffee%20to%20Rohit%20Mallick

        string upiUri =
            "upi://pay" +
            $"?pa={Uri.EscapeDataString(UpiId)}" +
            $"&pn={Uri.EscapeDataString(PayeeName)}" +
            $"&am={amount:0.00}" +
            "&cu=INR" +
            $"&tn={Uri.EscapeDataString(
                "Coffee to Rohit Mallick")}";


        // -------------------------------------------------
        // Generate QR
        // -------------------------------------------------

        using var qrGenerator =
            new QRCodeGenerator();


        using QRCodeData qrData =
            qrGenerator.CreateQrCode(
                upiUri,
                QRCodeGenerator.ECCLevel.M);


        using var qrCode =
            new PngByteQRCode(qrData);


        byte[] qrBytes =
            qrCode.GetGraphic(
                10,
                drawQuietZones: true);


        // -------------------------------------------------
        // Convert PNG bytes to WPF BitmapImage
        // -------------------------------------------------

        var image =
            new BitmapImage();


        using var stream =
            new MemoryStream(qrBytes);


        image.BeginInit();

        image.CacheOption =
            BitmapCacheOption.OnLoad;

        image.StreamSource =
            stream;

        image.EndInit();

        image.Freeze();


        // -------------------------------------------------
        // Display QR
        // -------------------------------------------------

        PaymentQrImage.Source =
            image;
    }


    // =====================================================
    // CHANGE AMOUNT
    // =====================================================

    private void ChangeAmount_Click(
        object sender,
        RoutedEventArgs e)
    {
        PaymentScreen.Visibility =
            Visibility.Collapsed;

        AmountScreen.Visibility =
            Visibility.Visible;


        updatingAmount = true;

        CustomAmountTextBox.Text =
            selectedAmount.ToString("0");

        updatingAmount = false;


        UpdatePayButton();


        CustomAmountTextBox.Focus();

        CustomAmountTextBox.SelectAll();
    }


    // =====================================================
    // CLOSE
    // =====================================================

    private void Close_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}