using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Auth;

public partial class RegisterPage : ContentPage
{
    private readonly AuthService _authService;

    public RegisterPage(AuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var lastName = LastNameEntry.Text?.Trim();
        var firstName = FirstNameEntry.Text?.Trim();
        var middleName = MiddleNameEntry.Text?.Trim();
        var login = LoginEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        // Валидация
        if (string.IsNullOrEmpty(lastName) ||
            string.IsNullOrEmpty(firstName) ||
            string.IsNullOrEmpty(login) ||
            string.IsNullOrEmpty(password))
        {
            ShowError("Заполните все обязательные поля (*)");
            return;
        }

        if (login.Length < 4)
        {
            ShowError("Логин должен содержать минимум 4 символа");
            return;
        }

        if (password.Length < 6)
        {
            ShowError("Пароль должен содержать минимум 6 символов");
            return;
        }

        if (password != confirmPassword)
        {
            ShowError("Пароли не совпадают");
            return;
        }

        try
        {
            SetLoading(true);
            ErrorLabel.IsVisible = false;

            await _authService.RegisterAsync(
                lastName, firstName,
                string.IsNullOrEmpty(middleName) ? null : middleName,
                login, password);

            // После регистрации сразу на главный экран
            await Shell.Current.GoToAsync("//main");
        }
        catch (Exception ex)
        {
            // Типичная ошибка — логин уже занят
            if (ex.Message.Contains("duplicate") || ex.Message.Contains("unique"))
                ShowError("Этот логин уже занят, выберите другой");
            else
                ShowError($"Ошибка: {ex.Message}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    private async void OnLoginTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//auth/login");
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        RegisterButton.IsEnabled = !isLoading;
    }
}