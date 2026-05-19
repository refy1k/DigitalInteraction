using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Auth;

public partial class ForgotPasswordPage : ContentPage
{
    private readonly VerificationService _verificationService;

    public ForgotPasswordPage(VerificationService verificationService)
    {
        InitializeComponent();
        _verificationService = verificationService;
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();

        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
        {
            ErrorLabel.Text = "Введите корректный email";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            SetLoading(true);
            ErrorLabel.IsVisible = false;
            SuccessLabel.IsVisible = false;

            var success = await _verificationService.ResetPasswordAsync(email);

            if (!success)
            {
                ErrorLabel.Text = "Аккаунт с таким email не найден";
                ErrorLabel.IsVisible = true;
                return;
            }

            SuccessLabel.Text = "✅ Новый пароль отправлен на почту!";
            SuccessLabel.IsVisible = true;
            SendButton.IsEnabled = false;

            // Через 3 секунды возвращаемся на логин
            await Task.Delay(3000);
            await Shell.Current.GoToAsync("//auth/login");
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Ошибка отправки: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnBackTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//auth/login");

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        SendButton.IsEnabled = !isLoading;
    }
}