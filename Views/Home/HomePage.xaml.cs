using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using DigitalInteraction.Services;
using DigitalInteraction.Views.Appeals;
using DigitalInteraction.Views.Applications;
using DigitalInteraction.Views.Profile;
using DigitalInteraction.Views.Services;
using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;

namespace DigitalInteraction.Views.Home;

public partial class HomePage : ContentPage
{
    private readonly AppealService           _appealService;
    private readonly ApplicationService      _applicationService;
    private readonly MunicipalServiceService _serviceService;
    private readonly HttpClient              _httpClient = new();

    public HomePage(
        AppealService appealService,
        ApplicationService applicationService,
        MunicipalServiceService serviceService)
    {
        InitializeComponent();
        _appealService      = appealService;
        _applicationService = applicationService;
        _serviceService     = serviceService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAllAsync();
    }

    private async Task LoadAllAsync()
    {
        var citizen = SessionManager.CurrentCitizen;
        GreetingLabel.Text = citizen is not null
            ? $"Здравствуйте, {citizen.FirstName}!"
            : "Добро пожаловать!";
        DateLabel.Text = DateTime.Now.ToString("dddd, d MMMM yyyy",
            new System.Globalization.CultureInfo("ru-RU"));

        await Task.WhenAll(
            LoadWeatherAsync(),
            LoadStatsAsync(),
            LoadActiveAppealsAsync(),
            LoadNewsAsync()
        );
    }

    // ── Погода ────────────────────────────────────────────────
    private async Task LoadWeatherAsync()
    {
        try
        {
            WeatherLoading.IsVisible = true;
            WeatherLoading.IsRunning = true;
            WeatherWidget.IsVisible  = false;

            var url = $"{AppConstants.WeatherApiUrl}" +
                      $"?q={AppConstants.WeatherCity}" +
                      $"&appid={AppConstants.WeatherApiKey}" +
                      $"&units=metric&lang=ru";

            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var temp  = root.GetProperty("main").GetProperty("temp").GetDouble();
            var wind  = root.GetProperty("wind").GetProperty("speed").GetDouble();
            var desc  = root.GetProperty("weather")[0]
                           .GetProperty("description").GetString();

            WeatherTempLabel.Text = $"{temp:F0}°C";
            WeatherDescLabel.Text = char.ToUpper(desc![0]) + desc[1..];
            WeatherWindLabel.Text = $"💨 {wind:F1} м/с";

            WeatherWidget.IsVisible = true;
        }
        catch (Exception ex)
        {
            WeatherErrorLabel.Text      = "Погода недоступна";
            WeatherErrorLabel.IsVisible = true;
        }
        finally
        {
            WeatherLoading.IsVisible = false;
            WeatherLoading.IsRunning = false;
        }
    }

    // ── Статистика ────────────────────────────────────────────
    private async Task LoadStatsAsync()
    {
        try
        {
            if (SessionManager.CurrentCitizenId is null) return;
            var id = SessionManager.CurrentCitizenId.Value;

            var appeals      = await _appealService.GetMyAppealsAsync(id);
            var applications  = await _applicationService.GetMyApplicationsAsync(id);
            var services     = await _serviceService.GetMyRequestsAsync(id);

            AppealsCountLabel.Text      = appeals.Count.ToString();
            ApplicationsCountLabel.Text = applications.Count.ToString();
            ServicesCountLabel.Text     = services.Count.ToString();
        }
        catch { }
    }

    // ── Активные обращения ────────────────────────────────────
    private async Task LoadActiveAppealsAsync()
    {
        try
        {
            if (SessionManager.CurrentCitizenId is null) return;

            var appeals = await _appealService
                .GetMyAppealsAsync(SessionManager.CurrentCitizenId.Value);

            var active = appeals
                .Where(a => a.AppealStatusId == 1 || a.AppealStatusId == 2)
                .Take(3)
                .ToList();

            ActiveAppealsLayout.Children.Clear();

            if (active.Count == 0)
            {
                NoActiveAppealsLabel.IsVisible = true;
                return;
            }

            NoActiveAppealsLabel.IsVisible = false;

            foreach (var appeal in active)
            {
                var border = new Border
                {
                    BackgroundColor = Colors.White,
                    StrokeThickness = 0,
                    Padding         = new Thickness(14, 10)
                };
                border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                    { CornerRadius = 10 };

                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Auto }
                    },
                    RowDefinitions =
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto }
                    },
                    RowSpacing = 3
                };

                grid.Add(new Label
                {
                    Text           = appeal.Title,
                    FontSize       = 14,
                    FontAttributes = FontAttributes.Bold,
                    TextColor      = Color.FromArgb("#212121"),
                    LineBreakMode  = LineBreakMode.TailTruncation
                }, 0, 0);

                var statusColor = appeal.AppealStatusId == 1
                    ? Color.FromArgb("#1565C0")
                    : Color.FromArgb("#E65100");
                var statusText = appeal.AppealStatusId == 1 ? "Новое" : "В обработке";

                var statusBorder = new Border
                {
                    BackgroundColor = statusColor,
                    StrokeThickness = 0,
                    Padding         = new Thickness(8, 3)
                };
                statusBorder.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
                    { CornerRadius = 10 };
                statusBorder.Content = new Label
                {
                    Text      = statusText,
                    FontSize  = 11,
                    TextColor = Colors.White
                };

                grid.Add(statusBorder, 1, 0);
                grid.Add(new Label
                {
                    Text      = appeal.AppealNumber,
                    FontSize  = 12,
                    TextColor = Color.FromArgb("#9E9E9E")
                }, 0, 1);

                border.Content = grid;

                var appealId = appeal.Id;
                border.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                        await Shell.Current.GoToAsync(
                            $"{nameof(AppealDetailPage)}?id={appealId}"))
                });

                ActiveAppealsLayout.Children.Add(border);
            }
        }
        catch { }
    }

    // ── Новости RSS ───────────────────────────────────────────
    private async Task LoadNewsAsync()
    {
        try
        {
            NewsLoading.IsVisible    = true;
            NewsLoading.IsRunning    = true;
            NewsContent.IsVisible    = false;
            NewsErrorLabel.IsVisible = false;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            var xml = await _httpClient.GetStringAsync(AppConstants.NewsRssUrl);
            var doc = XDocument.Parse(xml);

            var items = doc.Descendants("item")
                           .Take(4)
                           .Select(i => new
                           {
                               Title = i.Element("title")?.Value ?? "",
                               Link  = i.Element("link")?.Value  ?? "",
                               Date  = i.Element("pubDate")?.Value ?? ""
                           })
                           .ToList();

            NewsContent.Children.Clear();

            foreach (var item in items)
            {
                if (NewsContent.Children.Count > 0)
                    NewsContent.Children.Add(new BoxView
                    {
                        HeightRequest = 1,
                        Color         = Color.FromArgb("#F0F0F0"),
                        Margin        = new Thickness(0, 8)
                    });

                var titleLabel = new Label
                {
                    Text            = item.Title,
                    FontSize        = 13,
                    TextColor       = Color.FromArgb("#1A237E"),
                    TextDecorations = TextDecorations.Underline
                };

                var link = item.Link;
                titleLabel.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                        await Launcher.OpenAsync(new Uri(link)))
                });

                NewsContent.Children.Add(titleLabel);
                NewsContent.Children.Add(new Label
                {
                    Text      = item.Date,
                    FontSize  = 11,
                    TextColor = Color.FromArgb("#9E9E9E"),
                    Margin    = new Thickness(0, 2, 0, 0)
                });
            }

            NewsContent.IsVisible = true;
        }
        catch
        {
            NewsErrorLabel.Text      = "Не удалось загрузить новости";
            NewsErrorLabel.IsVisible = true;
        }
        finally
        {
            NewsLoading.IsVisible = false;
            NewsLoading.IsRunning = false;
        }
    }

    // ── Быстрые действия ──────────────────────────────────────
    private async void OnNewAppealTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(nameof(CreateAppealPage));

    private async void OnNewApplicationTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(nameof(CreateApplicationPage));

    private async void OnServicesTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(nameof(MunicipalServicesPage));

    private async void OnProfileTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(nameof(ProfilePage));

    private async void OnPhoneTapped(object sender, TappedEventArgs e) =>
        await Launcher.OpenAsync("tel:+73420000000");

    private async void OnEmailTapped(object sender, TappedEventArgs e) =>
        await Launcher.OpenAsync("mailto:admin@berezovsky.ru");

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadAllAsync();
        RefreshView.IsRefreshing = false;
    }
}