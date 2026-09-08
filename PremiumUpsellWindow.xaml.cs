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
            ? (IsIndia ? 299m : 9m)
            : (IsIndia ? 199m : 6m);

    private void ApplyMarketPricing()
    {
        if (IsIndia)
        {
            SixMonthPriceText.Text = "Minimum \u20B9199";
            YearPriceText.Text = "Minimum \u20B9299";
            CurrencyText.Text = "\u20B9";
        }
        else
        {
            SixMonthPriceText.Text = "Minimum $6";
            YearPriceText.Text = "Minimum $9";
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
                SetStatus(
                    "A previous checkout is still pending.");
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
            var parsed = new MailAddress(email);

            return string.Equals(
                parsed.Address,
                email.Trim(),
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
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
                NumberStyles.Number,
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

        if (amount > 100000m)
        {
            SetStatus(
                "Please enter a smaller contribution amount.",
                true);

            return false;
        }

        return true;
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
                "Complete payment in Razorpay. " +
                "Lucky Dangle will detect it automatically.");

            for (var attempt = 0; attempt < 160; attempt++)
            {
                await Task.Delay(
                    TimeSpan.FromSeconds(3),
                    pollCts.Token);

                var status =
                    await PremiumPurchaseService.GetStatusAsync(
                        checkout.CheckoutId,
                        pollCts.Token);

                if (!status.IsActive)
                    continue;

                PendingPremiumCheckoutStore.Clear();

                ApplySuccessfulEntitlement(status);
                RestoreAfterBrowserCheckout();
                return;
            }

            RestoreAfterBrowserCheckout();

            SetStatus(
                "Payment is still pending. " +
                "You can close this window and check again later.");
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
                RestoreCode = status.RestoreCode
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
        previousWindowStates.Clear();

        foreach (Window window in Application.Current.Windows)
        {
            if (!window.IsVisible)
                continue;

            previousTopmost[window] = window.Topmost;
            previousWindowStates[window] = window.WindowState;

            window.Topmost = false;

            if (window == this || window == Owner)
                window.WindowState = WindowState.Minimized;
        }
    }

    private void RestoreAfterBrowserCheckout()
    {
        if (!browserModeActive)
            return;

        browserModeActive = false;

        foreach (var pair in previousWindowStates)
        {
            try
            {
                if (!pair.Key.IsLoaded)
                    continue;

                pair.Key.WindowState =
                    pair.Value == WindowState.Minimized
                        ? WindowState.Normal
                        : pair.Value;
            }
            catch
            {
            }
        }

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

        previousWindowStates.Clear();
        previousTopmost.Clear();

        if (IsLoaded)
        {
            WindowState = WindowState.Normal;
            Show();
            Activate();
        }
    }

    private void SetBusy(bool busy)
    {
        UnlockButton.IsEnabled = !busy;
        EmailTextBox.IsEnabled = !busy;
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