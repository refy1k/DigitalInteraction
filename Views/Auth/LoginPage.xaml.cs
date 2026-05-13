using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Auth;

public partial class LoginPage : ContentPage
{
    private readonly AuthService _authService;

    public LoginPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var login = LoginEntry.Text?.Trim();
        var password = PasswordEntry.Text;

        // Валидация
        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            ErrorLabel.Text = "Заполните все поля";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            SetLoading(true);
            ErrorLabel.IsVisible = false;

            var citizen = await _authService.LoginAsync(login, password);

            if (citizen is null)
            {
                ErrorLabel.Text = "Неверный логин или пароль";
                ErrorLabel.IsVisible = true;
                return;
            }

            // Переход на главный экран
            await Shell.Current.GoToAsync("//main");
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Ошибка: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnRegisterTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//auth/register");
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoginButton.IsEnabled = !isLoading;
    }
}