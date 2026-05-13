using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Applications;

public partial class ApplicationsListPage : ContentPage
{
    private readonly ApplicationService _applicationService;

    public ApplicationsListPage(ApplicationService applicationService)
    {
        InitializeComponent();
        _applicationService = applicationService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadApplicationsAsync();
    }

    private async Task LoadApplicationsAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            ApplicationsCollection.IsVisible = false;
            EmptyView.IsVisible = false;

            var applications = await _applicationService
                .GetMyApplicationsAsync(SessionManager.CurrentCitizenId!.Value);

            if (applications.Count == 0)
            {
                EmptyView.IsVisible = true;
                return;
            }

            ApplicationsCollection.ItemsSource = BuildItems(applications);
            ApplicationsCollection.IsVisible = true;
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

    private static List<ApplicationDisplayItem> BuildItems(
        List<ServiceApplication> applications)
    {
        return applications.Select(a => new ApplicationDisplayItem
        {
            Id = a.Id,
            ApplicationNumber = a.ApplicationNumber,
            Title = a.Title,
            CreationDate = a.CreationDate,
            StatusText = GetStatusText(a.ApplicationStatusId),
            StatusColor = GetStatusColor(a.ApplicationStatusId)
        }).ToList();
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

    private async void OnApplicationTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is ApplicationDisplayItem item)
            await Shell.Current.GoToAsync(
                $"{nameof(ApplicationDetailPage)}?id={item.Id}");
    }

    private async void OnCreateClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CreateApplicationPage));
    }
}

public class ApplicationDisplayItem
{
    public int Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public Color StatusColor { get; set; }
}