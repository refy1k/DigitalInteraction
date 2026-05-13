using DigitalInteraction.Helpers;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Appeals;

public partial class CreateAppealPage : ContentPage
{
    private readonly AppealService _appealService;

    public CreateAppealPage(AppealService appealService)
    {
        InitializeComponent();
        _appealService = appealService;
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
            ErrorLabel.Text = "Текст обращения должен содержать минимум 20 символов";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            SetLoading(true);
            ErrorLabel.IsVisible = false;

            await _appealService.CreateAsync(
                SessionManager.CurrentCitizenId!.Value,
                title,
                text);

            await DisplayAlert("Успешно", "Обращение отправлено!", "OK");
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