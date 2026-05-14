using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Services;

[QueryProperty(nameof(CategoryId), "categoryId")]
[QueryProperty(nameof(CategoryName), "categoryName")]
[QueryProperty(nameof(ServiceId), "serviceId")]
public partial class MunicipalServiceDetailPage : ContentPage
{
    private readonly MunicipalServiceService _serviceService;
    private MunicipalService? _selectedService;

    private int _categoryId;
    private string _categoryName = string.Empty;
    private int _serviceId;

    public int CategoryId
    {
        get => _categoryId;
        set { _categoryId = value; LoadCategoryAsync(value).ConfigureAwait(false); }
    }

    public string CategoryName
    {
        get => _categoryName;
        set { _categoryName = Uri.UnescapeDataString(value); Title = _categoryName; }
    }

    public int ServiceId
    {
        get => _serviceId;
        set { _serviceId = value; LoadServiceAsync(value).ConfigureAwait(false); }
    }

    public MunicipalServiceDetailPage(MunicipalServiceService serviceService)
    {
        InitializeComponent();
        _serviceService = serviceService;
    }

    // Загрузка списка услуг категории
    private async Task LoadCategoryAsync(int categoryId)
    {
        try
        {
            var services = await _serviceService
                .GetServicesByCategoryAsync(categoryId);

            ServicesLayout.Children.Clear();

            foreach (var service in services)
            {
                var border = new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 0,
                    Padding = new Thickness(16, 14)
                };
                border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                { CornerRadius = 10 };

                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    }
                };

                var info = new VerticalStackLayout { Spacing = 4 };
                info.Children.Add(new Label
                {
                    Text = service.Name,
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#212121")
                });
                info.Children.Add(new Label
                {
                    Text = $"⏱ {service.DurationDays} дней",
                    FontSize = 12,
                    TextColor = Color.FromArgb("#9E9E9E")
                });

                grid.Add(info, 0, 0);
                grid.Add(new Label
                {
                    Text = "›",
                    FontSize = 24,
                    TextColor = Color.FromArgb("#9E9E9E"),
                    VerticalOptions = LayoutOptions.Center
                }, 1, 0);

                border.Content = grid;

                var svc = service;
                border.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => ShowRequestForm(svc))
                });

                ServicesLayout.Children.Add(border);
            }

            ServicesLayout.IsVisible = true;
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

    // Загрузка конкретной услуги (из поиска)
    private async Task LoadServiceAsync(int serviceId)
    {
        try
        {
            var services = await _serviceService
                .GetServicesByCategoryAsync(0);
            // Ищем через все категории
            var all = new List<MunicipalService>();
            var cats = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
            foreach (var c in cats)
            {
                var s = await _serviceService.GetServicesByCategoryAsync(c);
                all.AddRange(s);
            }
            var service = all.FirstOrDefault(s => s.Id == serviceId);
            if (service is not null) ShowRequestForm(service);
        }
        catch { }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private void ShowRequestForm(MunicipalService service)
    {
        _selectedService = service;
        Title = service.Name;

        ServiceNameLabel.Text = service.Name;
        ServiceDescLabel.Text = service.Description;
        ServiceDurationLabel.Text = $"⏱ Срок оказания: {service.DurationDays} рабочих дней";
        ServiceDocsLabel.Text = string.IsNullOrEmpty(service.Documents)
            ? string.Empty
            : $"📋 Необходимые документы:\n{service.Documents}";

        ServicesLayout.IsVisible = false;
        RequestFormLayout.IsVisible = true;

        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        if (_selectedService is null) return;

        try
        {
            SavingIndicator.IsVisible = true;
            SavingIndicator.IsRunning = true;
            SubmitButton.IsEnabled = false;
            ErrorLabel.IsVisible = false;

            await _serviceService.CreateRequestAsync(
                SessionManager.CurrentCitizenId!.Value,
                _selectedService.Id,
                CommentEditor.Text?.Trim());

            await DisplayAlert("Успешно",
                $"Заявка на услугу «{_selectedService.Name}» подана!\n" +
                $"Срок рассмотрения: {_selectedService.DurationDays} дней.", "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Ошибка: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            SavingIndicator.IsVisible = false;
            SavingIndicator.IsRunning = false;
            SubmitButton.IsEnabled = true;
        }
    }
}