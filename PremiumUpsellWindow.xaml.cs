using System.Globalization;
using System.Net.Mail;
using System.Windows;
using LuckyDangle.Services;

namespace LuckyDangle;

public partial class PremiumUpsellWindow : Window
{
    private bool _premiumWasAlreadyActiveOnOpen;
    private CancellationTokenSource? pollCts;

    private readonly Dictionary<Window, bool> previousTopmost = new();
    private readonly Dictionary<Window, WindowState> previousWindowStates = new();
    private bool browserModeActive;
    private bool normalizingAmountText;
    private PremiumRuntimeConfig premiumConfig = new();
    private ProviderRuntimeConfig providerConfig = new();
    private string purchaseVerificationToken = "";
    private string verifiedPurchaseEmail = "";

    public PremiumUpsellWindow()
    {
        InitializeComponent();

        var existingEntitlement = PremiumEntitlementStore.Load();
        _premiumWasAlreadyActiveOnOpen =
            existingEntitlement is not null &&
            existingEntitlement.ExpiresAtUtc > DateTime.UtcNow;
        ApplyMarketPricing();

        var saved = PremiumEntitlementStore.Load();
        if (saved is not null)
        {
            EmailTextBox.Text = saved.Email;
            RestoreEmailTextBox.Text = saved.Email;
        }
    }

    private bool IsIndia =>
        PremiumPurchaseService.GetMarketCode() == "IN";

    private decimal MinimumAmount =>
        YearPlan.IsChecked == true
            ? (IsIndia
                ? premiumConfig.India12mMin
                : premiumConfig.International12mMin)
            : (IsIndia
                ? premiumConfig.India6mMin
                : premiumConfig.International6mMin);

    private decimal MaximumAmount =>
        IsIndia
            ? premiumConfig.IndiaMax
            : premiumConfig.InternationalMax;
private void ApplyMarketPricing()
    {
        if (IsIndia)
        {
            SixMonthPriceText.Text = $"Minimum \u20B9{premiumConfig.India6mMin:0.##}";
            YearPriceText.Text = $"Minimum \u20B9{premiumConfig.India12mMin:0.##}";
            CurrencyText.Text = "\u20B9";
        }
        else
        {
            SixMonthPriceText.Text = $"Minimum ${premiumConfig.International6mMin:0.##}";
            YearPriceText.Text = $"Minimum ${premiumConfig.International12mMin:0.##}";
            CurrencyText.Text = "$";
        }

        SetMinimumAmount();
    }

    private void Plan_Checked(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
            return;

        SetMinimumAmount();
    }

    private void SetMinimumAmount()
    {
        var minimum = MinimumAmount;

        AmountTextBox.Text =
            minimum.ToString(
                "0.##",
                CultureInfo.InvariantCulture);

        AmountHintText.Text =
            IsIndia
                ? $"Minimum \u20B9{minimum:0}. You may contribute more if you wish."
                : $"Minimum ${minimum:0}. You may contribute more if you wish.";
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var runtimeConfig =
                await RuntimeConfigService.GetAsync();

            premiumConfig = runtimeConfig.Premium;
            providerConfig = runtimeConfig.Providers;

            RefundPolicyText.Text =
                "Refund Policy: " +
                runtimeConfig.RefundPolicyText;

            PrivacyUrlText.Text =
                "Privacy: " +
                runtimeConfig.PrivacyUrl;

            SupportEmailText.Text =
                "Support: " +
                runtimeConfig.SupportEmail;
            ApplyMarketPricing();

            var providerEnabled =
                IsIndia
                    ? providerConfig.RazorpayEnabled
                    : providerConfig.DodoEnabled;

            if (!providerEnabled)
            {
                UnlockButton.IsEnabled = false;
                SetStatus(
                    IsIndia
                        ? "Premium payments are temporarily unavailable."
                        : "International Premium payments are temporarily unavailable.",
                    true);
            }
        }
        catch
        {
            // Keep safe local defaults.
        }

        var active = PremiumEntitlementStore.Load();

        if (active is not null &&
            active.ExpiresAtUtc > DateTime.UtcNow)
        {
            ShowSuccess(active.ExpiresAtUtc);
            return;
        }

        var pending = PendingPremiumCheckoutStore.Load();
        if (pending is null)
            return;

        if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            EmailTextBox.Text = pending.Email;

        try
        {
            SetStatus("Checking your previous payment\u2026");

            var status =
                await PremiumPurchaseService.GetStatusAsync(
                    pending.CheckoutId);

            if (status.IsActive &&
                status.ExpiresAtUtc.HasValue)
            {
                ApplySuccessfulEntitlement(status);
                PendingPremiumCheckoutStore.Clear();
            }
            else
            {
                var priorState =
                    status.Status?.Trim().ToLowerInvariant() ?? "";

                if (
                    priorState == "failed" ||
                    priorState == "checkout_failed")
                {
                    PendingPremiumCheckoutStore.Clear();
                    SetStatus(
                        "Your previous payment was not completed. You can try again.",
                        true);
                }
                else if (
                    priorState == "cancelled" ||
                    priorState == "canceled")
                {
                    PendingPremiumCheckoutStore.Clear();
                    SetStatus(
                        "Your previous payment was cancelled. You can start a new payment when ready.");
                }
                else
                {
                    SetStatus(
                        "Your previous payment has not been confirmed yet. You can try again or check later.");
                }
            }
        }
        catch
        {
            SetStatus("");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var value = email.Trim();
            var parsed = new MailAddress(value);

            if (!string.Equals(
                    parsed.Address,
                    value,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var at = value.LastIndexOf('@');
            if (at <= 0 || at >= value.Length - 1)
                return false;

            var domain = value[(at + 1)..];

            if (domain.Length > 253 ||
                domain.StartsWith('.') ||
                domain.EndsWith('.') ||
                domain.StartsWith('-') ||
                domain.EndsWith('-') ||
                !domain.Contains('.'))
            {
                return false;
            }

            var labels = domain.Split('.');

            if (labels.Any(label =>
                    string.IsNullOrWhiteSpace(label) ||
                    label.StartsWith('-') ||
                    label.EndsWith('-') ||
                    label.Any(ch =>
                        !(char.IsLetterOrDigit(ch) || ch == '-'))))
            {
                return false;
            }

            var tld = labels[^1];

            return tld.Length >= 2 &&
                   tld.All(char.IsLetter);
        }
        catch
        {
            return false;
        }
    }

    private void AmountTextBox_PreviewTextInput(
        object sender,
        System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled =
            string.IsNullOrEmpty(e.Text) ||
            !e.Text.All(char.IsDigit);
    }

    private void AmountTextBox_TextChanged(
        object sender,
        System.Windows.Controls.TextChangedEventArgs e)
    {
        if (!IsLoaded || normalizingAmountText)
            return;

        normalizingAmountText = true;

        try
        {
            var original = AmountTextBox.Text;

            // Paste-safe: remove anything that is not a digit.
            var digits =
                new string(
                    original
                        .Where(char.IsDigit)
                        .ToArray());

            if (digits != original)
            {
                AmountTextBox.Text = digits;
                AmountTextBox.CaretIndex =
                    AmountTextBox.Text.Length;
            }

            if (string.IsNullOrWhiteSpace(digits))
                return;

            if (!decimal.TryParse(
                    digits,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out var amount))
            {
                AmountTextBox.Text =
                    MaximumAmount.ToString(
                        "0",
                        CultureInfo.InvariantCulture);

                AmountTextBox.CaretIndex =
                    AmountTextBox.Text.Length;
                return;
            }

            if (amount <= MaximumAmount)
                return;

            AmountTextBox.Text =
                MaximumAmount.ToString(
                    "0",
                    CultureInfo.InvariantCulture);

            AmountTextBox.CaretIndex =
                AmountTextBox.Text.Length;

            SetStatus(
                IsIndia
                    ? $"Maximum amount is \u20B9{MaximumAmount:0}."
                    : $"Maximum amount is ${MaximumAmount:0}.",
                true);
        }
        finally
        {
            normalizingAmountText = false;
        }
    }
    private bool TryReadAmount(out decimal amount)
    {
        amount = 0m;

        var raw =
            AmountTextBox.Text.Trim()
                .Replace(",", "");

        if (!decimal.TryParse(
                raw,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out amount))
        {
            SetStatus("Enter a valid amount.", true);
            return false;
        }

        if (amount < MinimumAmount)
        {
            SetStatus(
                IsIndia
                    ? $"Minimum amount is \u20B9{MinimumAmount:0}."
                    : $"Minimum amount is ${MinimumAmount:0}.",
                true);

            return false;
        }

        if (amount > MaximumAmount)
        {
            SetStatus(
                IsIndia
                    ? $"Maximum amount is \u20B9{MaximumAmount:0}."
                    : $"Maximum amount is ${MaximumAmount:0}.",
                true);

            return false;
        }

        return true;
    }

    private async void SendPurchaseOtp_Click(
        object sender,
        RoutedEventArgs e)
    {
        var email = EmailTextBox.Text.Trim();

        if (!IsValidEmail(email))
        {
            PurchaseOtpInfoText.Text =
                "Please enter a valid email address.";
            return;
        }

        try
        {
            SendPurchaseOtpButton.IsEnabled = false;
            PurchaseOtpInfoText.Text =
                "Sending verification code...";

            await PremiumPurchaseService.SendPurchaseOtpAsync(
                email);

            PurchaseOtpTextBox.IsEnabled = true;
            VerifyPurchaseOtpButton.IsEnabled = true;

            PurchaseOtpInfoText.Text =
                "A 6-digit code was sent to this email. " +
                "It is valid for 10 minutes.";
        }
        catch (Exception ex)
        {
            PurchaseOtpInfoText.Text = ex.Message;
        }
        finally
        {
            if (string.IsNullOrWhiteSpace(
                    purchaseVerificationToken))
            {
                SendPurchaseOtpButton.IsEnabled = true;
            }
        }
    }

    private async void VerifyPurchaseOtp_Click(
        object sender,
        RoutedEventArgs e)
    {
        var email = EmailTextBox.Text.Trim();
        var code = PurchaseOtpTextBox.Text.Trim();

        if (!IsValidEmail(email))
        {
            PurchaseOtpInfoText.Text =
                "Please enter a valid email address.";
            return;
        }

        if (code.Length != 6 ||
            !code.All(char.IsDigit))
        {
            PurchaseOtpInfoText.Text =
                "Enter the 6-digit verification code.";
            return;
        }

        try
        {
            VerifyPurchaseOtpButton.IsEnabled = false;
            PurchaseOtpInfoText.Text = "Verifying...";

            var result =
                await PremiumPurchaseService
                    .VerifyPurchaseOtpAsync(
                        email,
                        code);

            if (!result.Ok ||
                string.IsNullOrWhiteSpace(
                    result.VerificationToken))
            {
                PurchaseOtpInfoText.Text =
                    "Email verification failed.";
                VerifyPurchaseOtpButton.IsEnabled = true;
                return;
            }

            purchaseVerificationToken =
                result.VerificationToken;
            verifiedPurchaseEmail =
                email.Trim().ToLowerInvariant();

            EmailTextBox.IsReadOnly = true;
            PurchaseOtpTextBox.IsEnabled = false;
            SendPurchaseOtpButton.IsEnabled = false;
            VerifyPurchaseOtpButton.IsEnabled = false;

            PurchaseOtpInfoText.Text =
                "Email verified. You can now continue to payment.";
        }
        catch (Exception ex)
        {
            PurchaseOtpInfoText.Text = ex.Message;
            VerifyPurchaseOtpButton.IsEnabled = true;
        }
    }
    private async void Unlock_Click(
        object sender,
        RoutedEventArgs e)
    {
        var email = EmailTextBox.Text.Trim();

        if (!IsValidEmail(email))
        {
            SetStatus(
                "Please enter a valid email address.",
                true);
            return;
        }
        if (
            string.IsNullOrWhiteSpace(
                purchaseVerificationToken) ||
            !string.Equals(
                verifiedPurchaseEmail,
                email.Trim().ToLowerInvariant(),
                StringComparison.Ordinal))
        {
            SetStatus(
                "Please verify this email before payment.",
                true);
            return;
        }

        if (!TryReadAmount(out var amount))
            return;

        var plan =
            YearPlan.IsChecked == true
                ? "premium_12m"
                : "premium_6m";

        try
        {
            SetBusy(true);
            SetStatus("Preparing secure checkout\u2026");

            pollCts?.Cancel();
            pollCts = new CancellationTokenSource();

            var checkout =
                await PremiumPurchaseService.CreateCheckoutAsync(
                    email,
                    plan,
                    amount,
                    purchaseVerificationToken,
                    pollCts.Token);

            PendingPremiumCheckoutStore.Save(
                new PendingPremiumCheckout
                {
                    CheckoutId = checkout.CheckoutId,
                    CheckoutUrl = checkout.CheckoutUrl,
                    Email = email,
                    Plan = plan,
                    CreatedAtUtc = DateTime.UtcNow
                });

            PrepareForBrowserCheckout();

            PremiumPurchaseService.OpenCheckout(
                checkout.CheckoutUrl);

            SetStatus(
                "Complete payment securely in your browser. " +
                "Lucky Dangle will detect it automatically.");

            for (var attempt = 0; attempt < 30; attempt++)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(3),
                    pollCts.Token);

                var status =
                    await PremiumPurchaseService.GetStatusAsync(
                        checkout.CheckoutId,
                        pollCts.Token);
                if (status.IsActive)
                {
                    PendingPremiumCheckoutStore.Clear();
                    ApplySuccessfulEntitlement(status);
                    RestoreAfterBrowserCheckout();
                    return;
                }

                var paymentState =
                    status.Status?.Trim().ToLowerInvariant() ?? "";

                if (
                    paymentState == "failed" ||
                    paymentState == "checkout_failed" ||
                    paymentState == "cancelled" ||
                    paymentState == "canceled")
                {
                    PendingPremiumCheckoutStore.Clear();
                    RestoreAfterBrowserCheckout();

                    SetStatus(
                        paymentState == "failed" ||
                        paymentState == "checkout_failed"
                            ? "Payment was not completed. No charge was confirmed. You can try again."
                            : "Payment was cancelled. You can try again whenever you are ready.",
                        paymentState != "cancelled" &&
                        paymentState != "canceled");

                    return;
                }
            }

            RestoreAfterBrowserCheckout();

            SetStatus(
                "Payment has not been confirmed yet. " +
                "You can retry payment or check again later.");
        }
        catch (OperationCanceledException)
        {
            RestoreAfterBrowserCheckout();
        }
        catch (Exception ex)
        {
            RestoreAfterBrowserCheckout();
            SetStatus(ex.Message, true);
        }
        finally
        {
            if (PurchasePanel.Visibility == Visibility.Visible)
                SetBusy(false);
        }
    }

    private void ApplySuccessfulEntitlement(
        PremiumStatusResponse status)
    {
        if (!status.IsActive ||
            !status.ExpiresAtUtc.HasValue)
            return;

        var entitlement =
            new PremiumEntitlementSnapshot
            {
                Email = status.Email,
                ExpiresAtUtc = status.ExpiresAtUtc.Value,
                RestoreCode = status.RestoreCode,
                ValidationToken = status.ValidationToken,
                LastServerValidationAtUtc = DateTime.UtcNow
            };

        DangleAccessService.ApplyPremiumEntitlement(
            entitlement);

        ShowSuccess(entitlement.ExpiresAtUtc);
    }

    private void ShowSuccess(DateTime expiresAtUtc)
    {
        PurchasePanel.Visibility = Visibility.Collapsed;
        RestorePanel.Visibility = Visibility.Collapsed;
        SuccessPanel.Visibility = Visibility.Visible;

        SuccessCloseButton.Content =
            _premiumWasAlreadyActiveOnOpen
                ? "CLOSE"
                : "SAVE & CLOSE";

        SuccessExpiryText.Text =
            $"Premium active until " +
            $"{expiresAtUtc.ToLocalTime():dd MMM yyyy}";

        Height = 330;
    }

    private void Restore_Click(
        object sender,
        RoutedEventArgs e)
    {
        PurchasePanel.Visibility = Visibility.Collapsed;
        SuccessPanel.Visibility = Visibility.Collapsed;
        RestorePanel.Visibility = Visibility.Visible;

        if (string.IsNullOrWhiteSpace(
                RestoreEmailTextBox.Text))
        {
            RestoreEmailTextBox.Text =
                EmailTextBox.Text.Trim();
        }

        Height = 400;
    }

    private async void SendRestoreCode_Click(
        object sender,
        RoutedEventArgs e)
    {
        var email = RestoreEmailTextBox.Text.Trim();

        if (!IsValidEmail(email))
        {
            RestoreInfoText.Text =
                "Please enter a valid email address.";
            return;
        }

        try
        {
            SendRestoreCodeButton.IsEnabled = false;
            RestoreInfoText.Text =
                "Sending verification code...";

            await PremiumPurchaseService.SendRestoreOtpAsync(
                email);

            RestoreInfoText.Text =
                "If an active Premium purchase exists for this email, " +
                "a 6-digit code has been sent. " +
                "The code is valid for 10 minutes.";
        }
        catch (Exception ex)
        {
            RestoreInfoText.Text = ex.Message;
        }
        finally
        {
            SendRestoreCodeButton.IsEnabled = true;
        }
    }

    private async void VerifyRestore_Click(
        object sender,
        RoutedEventArgs e)
    {
        var email = RestoreEmailTextBox.Text.Trim();
        var code = RestoreCodeTextBox.Text.Trim();

        if (!IsValidEmail(email))
        {
            RestoreInfoText.Text =
                "Please enter a valid email address.";
            return;
        }

        if (code.Length != 6 ||
            !code.All(char.IsDigit))
        {
            RestoreInfoText.Text =
                "Enter the 6-digit verification code.";
            return;
        }

        try
        {
            VerifyRestoreButton.IsEnabled = false;
            RestoreInfoText.Text = "Verifying...";

            var status =
                await PremiumPurchaseService
                    .VerifyRestoreOtpAsync(
                        email,
                        code);

            if (!status.IsActive ||
                !status.ExpiresAtUtc.HasValue)
            {
                RestoreInfoText.Text =
                    "No active Premium purchase was found.";
                return;
            }

            _premiumWasAlreadyActiveOnOpen = false;
            ApplySuccessfulEntitlement(status);
        }
        catch (Exception ex)
        {
            RestoreInfoText.Text = ex.Message;
        }
        finally
        {
            VerifyRestoreButton.IsEnabled = true;
        }
    }
    private void BackToPurchase_Click(
        object sender,
        RoutedEventArgs e)
    {
        RestorePanel.Visibility = Visibility.Collapsed;
        SuccessPanel.Visibility = Visibility.Collapsed;
        PurchasePanel.Visibility = Visibility.Visible;
        Height = 535;
    }

    private void SaveClose_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (_premiumWasAlreadyActiveOnOpen)
        {
            DialogResult = false;
            Close();
            return;
        }

        DangleAccessService.RefreshFromLocalStorage();
        DialogResult = true;
        Close();
    }
    private void PrepareForBrowserCheckout()
    {
        if (browserModeActive)
            return;

        browserModeActive = true;
        previousTopmost.Clear();

        // Keep Lucky Dangle visible while the browser handles payment.
        foreach (Window window in Application.Current.Windows)
        {
            if (!window.IsVisible)
                continue;

            previousTopmost[window] = window.Topmost;
            window.Topmost = false;
        }
    }

    private void RestoreAfterBrowserCheckout()
    {
        if (!browserModeActive)
            return;

        browserModeActive = false;

        foreach (var pair in previousTopmost)
        {
            try
            {
                if (pair.Key.IsLoaded)
                    pair.Key.Topmost = pair.Value;
            }
            catch
            {
            }
        }

        previousTopmost.Clear();

        if (IsLoaded)
            Show();
    }

    private void SetBusy(bool busy)
    {
        UnlockButton.IsEnabled = !busy;
        EmailTextBox.IsEnabled = !busy;        if (!string.IsNullOrWhiteSpace(purchaseVerificationToken))
            EmailTextBox.IsReadOnly = true;
        AmountTextBox.IsEnabled = !busy;
        SixMonthPlan.IsEnabled = !busy;
        YearPlan.IsEnabled = !busy;
    }

    private void SetStatus(
        string message,
        bool error = false)
    {
        StatusText.Text = message;

        StatusText.Foreground =
            error
                ? System.Windows.Media.Brushes.IndianRed
                : System.Windows.Media.Brushes.LightGray;
    }

    private void Close_Click(
        object sender,
        RoutedEventArgs e)
    {
        pollCts?.Cancel();
        RestoreAfterBrowserCheckout();
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        pollCts?.Cancel();
        pollCts?.Dispose();
        RestoreAfterBrowserCheckout();
        base.OnClosed(e);
    }
}