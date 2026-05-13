using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Applications;

[QueryProperty(nameof(ApplicationId), "id")]
public partial class ApplicationDetailPage : ContentPage
{
    private readonly ApplicationService _applicationService;
    private int _applicationId;

    public int ApplicationId
    {
        get => _applicationId;
        set
        {
            _applicationId = value;
            LoadApplicationAsync(value).ConfigureAwait(false);
        }
    }

    public ApplicationDetailPage(ApplicationService applicationService)
    {
        InitializeComponent();
        _applicationService = applicationService;
    }

    private async Task LoadApplicationAsync(int id)
    {
        try
        {
            var app = await _applicationService.GetByIdAsync(id);
            if (app is null)
            {
                await DisplayAlert("Ошибка", "Заявка не найдена", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            NumberLabel.Text = app.ApplicationNumber;
            TitleLabel.Text = app.Title;
            DateLabel.Text = $"Подана: {app.CreationDate:dd.MM.yyyy HH:mm}";
            TextLabel.Text = app.DocumentText;

            StatusLabel.Text = GetStatusText(app.ApplicationStatusId);
            StatusFrame.BackgroundColor = GetStatusColor(app.ApplicationStatusId);

            if (!string.IsNullOrEmpty(app.ResponseText))
            {
                ResponseLabel.Text = app.ResponseText;
                ResponseDateLabel.Text = app.ResponseDate.HasValue
                    ? $"Дата ответа: {app.ResponseDate.Value:dd.MM.yyyy}"
                    : string.Empty;
                ResponseFrame.IsVisible = true;
            }

            DeleteButton.IsVisible = app.ApplicationStatusId == 1;
            ContentLayout.IsVisible = true;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Удаление",
            "Вы уверены что хотите удалить эту заявку?",
            "Удалить", "Отмена");

        if (!confirm) return;

        try
        {
            await _applicationService.DeleteAsync(_applicationId);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private static string GetStatusText(int statusId) => statusId switch
    {
        1 => "Новая",
        2 => "В обработке",
        3 => "Выполнена",
        4 => "Отклонена",
        _ => "Неизвестно"
    };

    private static Color GetStatusColor(int statusId) => statusId switch
    {
        1 => Color.FromArgb("#1565C0"),
        2 => Color.FromArgb("#E65100"),
        3 => Color.FromArgb("#2E7D32"),
        4 => Color.FromArgb("#B71C1C"),
        _ => Color.FromArgb("#757575")
    };
}