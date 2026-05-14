using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Services;

public partial class ServiceRequestsPage : ContentPage
{
    private readonly MunicipalServiceService _serviceService;

    public ServiceRequestsPage(MunicipalServiceService serviceService)
    {
        InitializeComponent();
        _serviceService = serviceService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRequestsAsync();
    }

    private async Task LoadRequestsAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            RequestsCollection.IsVisible = false;
            EmptyView.IsVisible = false;

            var requests = await _serviceService
                .GetMyRequestsAsync(SessionManager.CurrentCitizenId!.Value);

            if (requests.Count == 0)
            {
                EmptyView.IsVisible = true;
                return;
            }

            // Загружаем названия услуг
            var allServices = new Dictionary<int, string>();
            foreach (var r in requests)
            {
                if (!allServices.ContainsKey(r.ServiceId))
                {
                    var cats = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
                    foreach (var c in cats)
                    {
                        var svcs = await _serviceService.GetServicesByCategoryAsync(c);
                        var svc = svcs.FirstOrDefault(s => s.Id == r.ServiceId);
                        if (svc is not null)
                        {
                            allServices[r.ServiceId] = svc.Name;
                            break;
                        }
                    }
                }
            }

            RequestsCollection.ItemsSource = requests.Select(r =>
                new ServiceRequestDisplayItem
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    ServiceName = allServices.TryGetValue(r.ServiceId, out var name)
                        ? name : "Услуга",
                    CreationDate = r.CreationDate,
                    StatusText = GetStatusText(r.AppealStatusId),
                    StatusColor = GetStatusColor(r.AppealStatusId)
                }).ToList();

            RequestsCollection.IsVisible = true;
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

    private async void OnNewRequestClicked(object sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(MunicipalServicesPage));

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

public class ServiceRequestDisplayItem
{
    public int Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime CreationDate { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public Color StatusColor { get; set; }
}