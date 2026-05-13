using DigitalInteraction.Services;

using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Appeals;

[QueryProperty(nameof(AppealId), "id")]
public partial class AppealDetailPage : ContentPage
{
    private readonly AppealService _appealService;
    private int _appealId;

    public int AppealId
    {
        get => _appealId;
        set
        {
            _appealId = value;
            LoadAppealAsync(value).ConfigureAwait(false);
        }
    }

    public AppealDetailPage(AppealService appealService)
    {
        InitializeComponent();
        _appealService = appealService;
    }

    private async Task LoadAppealAsync(int id)
    {
        try
        {
            var appeal = await _appealService.GetByIdAsync(id);
            if (appeal is null)
            {
                await DisplayAlert("Ошибка", "Обращение не найдено", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Заполняем данные
            NumberLabel.Text = appeal.AppealNumber;
            TitleLabel.Text = appeal.Title;
            DateLabel.Text = $"Подано: {appeal.CreationDate:dd.MM.yyyy HH:mm}";
            TextLabel.Text = appeal.DocumentText;

            // Статус
            StatusLabel.Text = GetStatusText(appeal.AppealStatusId);
            StatusFrame.BackgroundColor = GetStatusColor(appeal.AppealStatusId);

            // Ответ
            if (!string.IsNullOrEmpty(appeal.ResponseText))
            {
                ResponseLabel.Text = appeal.ResponseText;
                ResponseDateLabel.Text = appeal.ResponseDate.HasValue
                    ? $"Дата ответа: {appeal.ResponseDate.Value:dd.MM.yyyy}"
                    : string.Empty;
                ResponseFrame.IsVisible = true;
            }

            // Кнопка удалить — только для статуса «Новое»
            DeleteButton.IsVisible = appeal.AppealStatusId == 1;

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
            "Вы уверены что хотите удалить это обращение?",
            "Удалить", "Отмена");

        if (!confirm) return;

        try
        {
            await _appealService.DeleteAsync(_appealId);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private static string GetStatusText(int statusId) => statusId switch
    {
        1 => "Новое",
        2 => "В обработке",
        3 => "Выполнено",
        4 => "Отклонено",
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