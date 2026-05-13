using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Appeals;

public partial class AppealsListPage : ContentPage
{
    private readonly AppealService _appealService;

    public AppealsListPage(AppealService appealService)
    {
        InitializeComponent();
        _appealService = appealService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAppealsAsync();
    }

    private async Task LoadAppealsAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            AppealsCollection.IsVisible = false;
            EmptyView.IsVisible = false;

            var appeals = await _appealService
                .GetMyAppealsAsync(SessionManager.CurrentCitizenId!.Value);

            if (appeals.Count == 0)
            {
                EmptyView.IsVisible = true;
                return;
            }

            AppealsCollection.ItemsSource = BuildAppealItems(appeals);
            AppealsCollection.IsVisible = true;
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

    // Преобразуем Appeal в отображаемый объект с текстом и цветом статуса
    private static List<AppealDisplayItem> BuildAppealItems(List<Appeal> appeals)
    {
        return appeals.Select(a => new AppealDisplayItem
        {
            Id = a.Id,
            AppealNumber = a.AppealNumber,
            Title = a.Title,
            CreationDate = a.CreationDate,
            StatusText = GetStatusText(a.AppealStatusId),
            StatusColor = GetStatusColor(a.AppealStatusId)
        }).ToList();
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

    private async void OnAppealTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is AppealDisplayItem item)
            await Shell.Current.GoToAsync(
                $"{nameof(AppealDetailPage)}?id={item.Id}");
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateAppealPage));
    }
}

// Вспомогательный класс для отображения
public class AppealDisplayItem
{
    public int Id { get; set; }
    public string AppealNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public Color StatusColor { get; set; }
}