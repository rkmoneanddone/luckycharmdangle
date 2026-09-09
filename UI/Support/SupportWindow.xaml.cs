using System;
using System.Diagnostics;
using System.Globalization;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LuckyDangle.Services;

namespace LuckyDangle.UI.Support;

public partial class SupportWindow : Window
{
    private readonly bool _isIndia;

    private decimal _minimumAmount;
    private decimal _maximumAmount;
    private decimal _selectedAmount;
    private bool _updatingAmount;
    private CancellationTokenSource? _paymentPolling;
    private bool _paymentCompleted;
    private WindowState? _ownerPreviousState;

    public SupportWindow()
    {
        InitializeComponent();

        _isIndia =
            SupportPurchaseService.GetMarketCode() == "IN";

        if (_isIndia)
        {
            _minimumAmount = 100m;
            _maximumAmount = 5000m;
            _selectedAmount = 200m;

            MarketText.Text =
                "India payment via Razorpay";

            Preset1Button.Content = "INR 100";
            Preset1Button.Tag = "100";

            Preset2Button.Content = "INR 150";
            Preset2Button.Tag = "150";

            Preset3Button.Content = "INR 200";
            Preset3Button.Tag = "200";

            AmountHelpText.Text =
                "Or enter another amount (INR 100 - INR 5,000)";

            CustomAmountTextBox.Text = "200";
        }
        else
        {
            _minimumAmount = 3m;
            _maximumAmount = 100m;
            _selectedAmount = 3m;

            MarketText.Text =
                "International payment via Dodo";

            Preset1Button.Content = "USD 3";
            Preset1Button.Tag = "3";

            Preset2Button.Content = "USD 5";
            Preset2Button.Tag = "5";

            Preset3Button.Content = "USD 10";
            Preset3Button.Tag = "10";

            AmountHelpText.Text =
                "Or enter another amount (USD 3 - USD 100)";

            CustomAmountTextBox.Text = "3";
        }

        UpdatePayButton();

        Loaded += SupportWindow_Loaded;
        Closed += SupportWindow_Closed;
    }

    private async void SupportWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        try
        {
            var config =
                await RuntimeConfigService.GetAsync();

            ApplyCoffeeRuntimeConfig(config);
        }
        catch
        {
            // Keep safe local defaults if remote config is unavailable.
        }

        var pending =
            PendingCoffeeCheckoutStore.LoadRecent();

        if (
            pending == null ||
            string.IsNullOrWhiteSpace(
                pending.CheckoutId))
            return;

        EmailTextBox.Text = pending.Email;

        _updatingAmount = true;
        CustomAmountTextBox.Text =
            pending.Amount.ToString(
                "0.##",
                CultureInfo.InvariantCulture);
        _updatingAmount = false;

        _selectedAmount = pending.Amount;
        UpdatePayButton();

        StatusText.Text =
            "Checking your recent coffee payment...";

        try
        {
            var status =
                await SupportPurchaseService.GetStatusAsync(
                    pending.CheckoutId);

            if (status.IsPaid)
            {
                _paymentCompleted = true;
                PendingCoffeeCheckoutStore.Clear();

                StatusText.Text =
                    "Payment successful. Thank you for supporting Lucky Dangle!";

                PayButton.IsEnabled = false;
                PayButton.Visibility = Visibility.Collapsed;

                return;
            }
        }
        catch
        {
        }

        // A previous unpaid/abandoned checkout should not make the
        // Coffee window look as if a payment page is currently open.
        PendingCoffeeCheckoutStore.Clear();
        StatusText.Text = "";
    }

    private void ApplyCoffeeRuntimeConfig(
        LuckyDanglePublicConfig config)
    {
        var presets =
            _isIndia
                ? config.Coffee.IndiaPresets
                : config.Coffee.InternationalPresets;

        if (presets is null || presets.Length < 3)
        {
            presets =
                _isIndia
                    ? [100m, 150m, 200m]
                    : [3m, 5m, 10m];
        }

        _minimumAmount =
            _isIndia ? config.Coffee.IndiaMin : config.Coffee.InternationalMin;

        _maximumAmount =
            _isIndia ? config.Coffee.IndiaMax : config.Coffee.InternationalMax;

        var defaultAmount =
            _isIndia
                ? config.Coffee.IndiaDefault
                : config.Coffee.InternationalDefault;

        if (defaultAmount < _minimumAmount ||
            defaultAmount > _maximumAmount)
        {
            defaultAmount = _minimumAmount;
        }

        Preset1Button.Content =
            _isIndia
                ? $"INR {presets[0]:0.##}"
                : $"USD {presets[0]:0.##}";
        Preset1Button.Tag =
            presets[0].ToString(
                CultureInfo.InvariantCulture);

        Preset2Button.Content =
            _isIndia
                ? $"INR {presets[1]:0.##}"
                : $"USD {presets[1]:0.##}";
        Preset2Button.Tag =
            presets[1].ToString(
                CultureInfo.InvariantCulture);

        Preset3Button.Content =
            _isIndia
                ? $"INR {presets[2]:0.##}"
                : $"USD {presets[2]:0.##}";
        Preset3Button.Tag =
            presets[2].ToString(
                CultureInfo.InvariantCulture);

        AmountHelpText.Text =
            _isIndia
                ? $"Or enter another amount (INR {_minimumAmount:0.##} - INR {_maximumAmount:N0})"
                : $"Or enter another amount (USD {_minimumAmount:0.##} - USD {_maximumAmount:0.##})";

        _selectedAmount = defaultAmount;

        _updatingAmount = true;
        CustomAmountTextBox.Text =
            defaultAmount.ToString(
                "0.##",
                CultureInfo.InvariantCulture);
        _updatingAmount = false;

        var providerEnabled =
            _isIndia
                ? config.Providers.RazorpayEnabled
                : config.Providers.DodoEnabled;

        PayButton.IsEnabled = providerEnabled;

        if (!providerEnabled)
        {
            StatusText.Text =
                "Coffee payments are temporarily unavailable.";
        }

        UpdatePayButton();
    }
    private void SupportWindow_Closed(
        object? sender,
        EventArgs e)
    {
        _paymentPolling?.Cancel();
        _paymentPolling?.Dispose();
        _paymentPolling = null;
    }

    private async Task PollCheckoutAsync(
        string checkoutId,
        TimeSpan duration)
    {
        _paymentPolling?.Cancel();
        _paymentPolling?.Dispose();

        _paymentPolling =
            new CancellationTokenSource();

        var token =
            _paymentPolling.Token;

        var deadline =
            DateTime.UtcNow + duration;

        try
        {
            while (
                !token.IsCancellationRequested &&
                DateTime.UtcNow < deadline)
            {
                try
                {
                    var status =
                        await SupportPurchaseService.GetStatusAsync(
                            checkoutId,
                            token);

                    if (status.IsPaid)
                    {
                        PendingCoffeeCheckoutStore.Clear();

                        StatusText.Text =
                            "Payment successful. Thank you for supporting Lucky Dangle!";

                        _paymentCompleted = true;
                        PayButton.IsEnabled = false;
                        PayButton.Visibility = Visibility.Collapsed;
                        RestoreAfterBrowserPayment();

                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch
                {
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(2),
                    token);
            }

            if (!token.IsCancellationRequested)
            {
                StatusText.Text =
                    "Payment was not confirmed yet. You can try again or close this window.";
                RestoreAfterBrowserPayment();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
    private void MinimizeForBrowserPayment()
    {
        if (Owner != null)
        {
            _ownerPreviousState = Owner.WindowState;
            Owner.WindowState = WindowState.Minimized;
        }

        WindowState = WindowState.Minimized;
    }

    private void RestoreAfterBrowserPayment()
    {
        if (Owner != null && _ownerPreviousState.HasValue)
        {
            Owner.WindowState = _ownerPreviousState.Value;
            _ownerPreviousState = null;
        }

        WindowState = WindowState.Normal;
        Activate();
    }
    private void PresetAmount_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (
            sender is Button button &&
            button.Tag is string value &&
            decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var amount))
        {
            _updatingAmount = true;

            CustomAmountTextBox.Text =
                amount.ToString(
                    "0.##",
                    CultureInfo.InvariantCulture);

            _updatingAmount = false;
            _selectedAmount = amount;

            UpdatePayButton();
        }
    }

    private void CustomAmountTextBox_PreviewTextInput(
        object sender,
        TextCompositionEventArgs e)
    {
        if (!Regex.IsMatch(e.Text, @"^[0-9.]$"))
        {
            e.Handled = true;
            return;
        }

        var current =
            CustomAmountTextBox.Text;

        var next =
            current.Remove(
                    CustomAmountTextBox.SelectionStart,
                    CustomAmountTextBox.SelectionLength)
                .Insert(
                    CustomAmountTextBox.SelectionStart,
                    e.Text);

        if (!Regex.IsMatch(
                next,
                @"^\d{0,5}(\.\d{0,2})?$"))
        {
            e.Handled = true;
            return;
        }

        if (
            decimal.TryParse(
                next,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var amount) &&
            amount > _maximumAmount)
        {
            e.Handled = true;
        }
    }

    private void CustomAmountTextBox_Pasting(
        object sender,
        DataObjectPastingEventArgs e)
    {
        if (!e.DataObject.GetDataPresent(typeof(string)))
        {
            e.CancelCommand();
            return;
        }

        var pasted =
            e.DataObject.GetData(typeof(string)) as string ?? "";

        if (!Regex.IsMatch(
                pasted,
                @"^\d{1,5}(\.\d{0,2})?$"))
        {
            e.CancelCommand();
        }
    }

    private void CustomAmountTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (!_updatingAmount)
            UpdatePayButton();
    }

    private void CustomAmountTextBox_LostFocus(
        object sender,
        RoutedEventArgs e)
    {
        if (!TryGetAmount(out var amount))
        {
            SetMinimumAmount();
            return;
        }

        if (amount < _minimumAmount)
        {
            SetMinimumAmount();
            return;
        }

        if (amount > _maximumAmount)
        {
            amount = _maximumAmount;

            _updatingAmount = true;

            CustomAmountTextBox.Text =
                amount.ToString(
                    "0.##",
                    CultureInfo.InvariantCulture);

            _updatingAmount = false;
        }

        _selectedAmount = amount;
        UpdatePayButton();
    }

    private bool TryGetAmount(out decimal amount)
    {
        return decimal.TryParse(
            CustomAmountTextBox.Text.Trim(),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out amount);
    }

    private void SetMinimumAmount()
    {
        _selectedAmount = _minimumAmount;

        _updatingAmount = true;

        CustomAmountTextBox.Text =
            _minimumAmount.ToString(
                "0.##",
                CultureInfo.InvariantCulture);

        _updatingAmount = false;

        UpdatePayButton();
    }

    private void UpdatePayButton()
    {
        if (PayButton == null)
            return;

        if (
            TryGetAmount(out var amount) &&
            amount >= _minimumAmount &&
            amount <= _maximumAmount)
        {
            PayButton.Content =
                _isIndia
                    ? $"Pay INR {amount:0.##}"
                    : $"Pay USD {amount:0.##}";
        }
        else
        {
            PayButton.Content = "Pay";
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var address = new MailAddress(email);

            if (!string.Equals(
                    address.Address,
                    email,
                    StringComparison.OrdinalIgnoreCase))
                return false;

            var at = email.LastIndexOf('@');

            if (at <= 0 || at >= email.Length - 1)
                return false;

            var domain = email[(at + 1)..];

            if (
                !domain.Contains('.') ||
                domain.StartsWith('.') ||
                domain.EndsWith('.'))
                return false;

            var parts = domain.Split('.');

            if (parts.Length < 2)
                return false;

            var tld = parts[^1];

            return
                tld.Length >= 2 &&
                tld.All(char.IsLetter);
        }
        catch
        {
            return false;
        }
    }

    private async void Pay_Click(
        object sender,
        RoutedEventArgs e)
    {
        var email = EmailTextBox.Text.Trim();

        if (!IsValidEmail(email))
        {
            StatusText.Text =
                "Please enter a valid email address.";
            return;
        }

        if (!TryGetAmount(out var amount))
        {
            SetMinimumAmount();
            return;
        }

        if (
            amount < _minimumAmount ||
            amount > _maximumAmount)
        {
            StatusText.Text =
                _isIndia
                    ? "Enter an amount between INR 100 and INR 5,000."
                    : "Enter an amount between USD 3 and USD 100.";
            return;
        }

        amount = Math.Round(
            amount,
            2,
            MidpointRounding.AwayFromZero);

        _selectedAmount = amount;

        try
        {
            PayButton.IsEnabled = false;
            StatusText.Text =
                "Opening secure payment...";

            var checkout =
                await SupportPurchaseService.CreateCheckoutAsync(
                    email,
                    amount);

            if (string.IsNullOrWhiteSpace(checkout.CheckoutUrl))
                throw new InvalidOperationException(
                    "Payment link was not returned.");

            PendingCoffeeCheckoutStore.Save(
                new PendingCoffeeCheckout
                {
                    CheckoutId = checkout.CheckoutId,
                    CheckoutUrl = checkout.CheckoutUrl,
                    Email = email,
                    Amount = amount,
                    CreatedAtUtc = DateTime.UtcNow
                });

            MinimizeForBrowserPayment();

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = checkout.CheckoutUrl,
                    UseShellExecute = true
                });

            StatusText.Text =
                _isIndia
                    ? "Razorpay opened in your browser. Waiting for verified payment..."
                    : "Dodo opened in your browser. Waiting for verified payment...";

            await PollCheckoutAsync(
                checkout.CheckoutId,
                TimeSpan.FromMinutes(10));
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
        }
        finally
        {
            if (!_paymentCompleted)
                PayButton.IsEnabled = true;
        }
    }

    private void Close_Click(
        object sender,
        RoutedEventArgs e)
    {
        Close();
    }
}