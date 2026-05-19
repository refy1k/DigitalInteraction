using DigitalInteraction.Helpers;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Profile;

public partial class EmailVerificationPage : ContentPage
{
    private readonly VerificationService _verificationService;
    private string _pendingEmail = string.Empty;

    public EmailVerificationPage(VerificationService verificationService)
    {
        InitializeComponent();
        _verificationService = verificationService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var citizen = SessionManager.CurrentCitizen;
        if (citizen is null) return;

        if (citizen.IsEmailVerified)
        {
            VerifiedBadge.IsVisible = true;
            EmailInputLayout.IsVisible = false;
            CodeInputLayout.IsVisible = false;
        }
        else
        {
            VerifiedBadge.IsVisible = false;
            EmailInputLayout.IsVisible = true;
        }
    }

    private async void OnSendCodeClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();

        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
        {
            EmailError.Text = "Введите корректный email";
            EmailError.IsVisible = true;
            return;
        }

        try
        {
            SetLoading(true);
            EmailError.IsVisible = false;

            _pendingEmail = email;

            await _verificationService.SendVerificationCodeAsync(
                SessionManager.CurrentCitizenId!.Value, email);

            CodeSentLabel.Text = $"Код отправлен на {email}. Проверьте папку «Входящие» и «Спам».";
            EmailInputLayout.IsVisible = false;
            CodeInputLayout.IsVisible = true;
        }
        catch (Exception ex)
        {
            EmailError.Text = $"Ошибка: {ex.Message}";
            EmailError.IsVisible = true;
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnVerifyClicked(object sender, EventArgs e)
    {
        var code = CodeEntry.Text?.Trim();

        if (string.IsNullOrEmpty(code) || code.Length != 6)
        {
            CodeError.Text = "Введите 6-значный код";
            CodeError.IsVisible = true;
            return;
        }

        try
        {
            SetLoading(true);
            CodeError.IsVisible = false;

            var success = await _verificationService.VerifyCodeAsync(
                SessionManager.CurrentCitizenId!.Value,
                _pendingEmail,
                code);

            if (!success)
            {
                CodeError.Text = "Неверный или истёкший код. Запросите новый.";
                CodeError.IsVisible = true;
                return;
            }

            await DisplayAlert("✅ Готово", "Email успешно подтверждён!", "OK");

            VerifiedBadge.IsVisible = true;
            CodeInputLayout.IsVisible = false;
            EmailInputLayout.IsVisible = false;
        }
        catch (Exception ex)
        {
            CodeError.Text = $"Ошибка: {ex.Message}";
            CodeError.IsVisible = true;
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnResendTapped(object sender, TappedEventArgs e)
    {
        if (string.IsNullOrEmpty(_pendingEmail)) return;

        try
        {
            SetLoading(true);
            await _verificationService.SendVerificationCodeAsync(
                SessionManager.CurrentCitizenId!.Value, _pendingEmail);
            await DisplayAlert("Готово", "Код отправлен повторно", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        SendCodeButton.IsEnabled = !isLoading;
        VerifyButton.IsEnabled = !isLoading;
    }
}