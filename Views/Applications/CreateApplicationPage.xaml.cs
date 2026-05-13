using DigitalInteraction.Helpers;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Applications;

public partial class CreateApplicationPage : ContentPage
{
    private readonly ApplicationService _applicationService;

    public CreateApplicationPage(ApplicationService applicationService)
    {
        InitializeComponent();
        _applicationService = applicationService;
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        var title = TitleEntry.Text?.Trim();
        var text = TextEditor.Text?.Trim();

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(text))
        {
            ErrorLabel.Text = "Заполните все обязательные поля";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (title.Length < 5)
        {
            ErrorLabel.Text = "Тема должна содержать минимум 5 символов";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (text.Length < 20)
        {
            ErrorLabel.Text = "Текст заявки должен содержать минимум 20 символов";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            SetLoading(true);
            ErrorLabel.IsVisible = false;

            await _applicationService.CreateAsync(
                SessionManager.CurrentCitizenId!.Value,
                title,
                text);

            await DisplayAlert("Успешно", "Заявка отправлена!", "OK");
            await Shell.Current.GoToAsync("..");
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

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        SubmitButton.IsEnabled = !isLoading;
    }
}