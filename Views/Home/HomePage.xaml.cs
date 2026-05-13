using System.Net.Http;
using System.Text.Json;
using System.Xml.Linq;
using DigitalInteraction.Helpers;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Home;

public partial class HomePage : ContentPage
{
    private readonly AppealService _appealService;
    private readonly ApplicationService _applicationService;
    private readonly HttpClient _httpClient = new();

    public HomePage(AppealService appealService, ApplicationService applicationService)
    {
        InitializeComponent();
        _appealService = appealService;
        _applicationService = applicationService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAllAsync();
    }

    private async Task LoadAllAsync()
    {
        // Приветствие
        var citizen = SessionManager.CurrentCitizen;
        GreetingLabel.Text = citizen is not null
            ? $"Здравствуйте, {citizen.FirstName}!"
            : "Добро пожаловать!";
        DateLabel.Text = DateTime.Now.ToString("dddd, d MMMM yyyy",
            new System.Globalization.CultureInfo("ru-RU"));

        // Загружаем параллельно
        await Task.WhenAll(
            LoadWeatherAsync(),
            LoadStatsAsync(),
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
            WeatherContent.IsVisible = false;
            WeatherErrorLabel.IsVisible = false;

            var url = $"{AppConstants.WeatherApiUrl}" +
                      $"?q={AppConstants.WeatherCity}" +
                      $"&appid={AppConstants.WeatherApiKey}" +
                      $"&units=metric&lang=ru";

            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var temp = root.GetProperty("main").GetProperty("temp").GetDouble();
            var feels = root.GetProperty("main").GetProperty("feels_like").GetDouble();
            var humidity = root.GetProperty("main").GetProperty("humidity").GetInt32();
            var wind = root.GetProperty("wind").GetProperty("speed").GetDouble();
            var desc = root.GetProperty("weather")[0].GetProperty("description").GetString();

            WeatherTempLabel.Text = $"{temp:F0}°C (ощущается {feels:F0}°C)";
            WeatherDescLabel.Text = char.ToUpper(desc![0]) + desc[1..];
            WeatherHumidityLabel.Text = $"💧 Влажность: {humidity}%";
            WeatherWindLabel.Text = $"💨 Ветер: {wind:F1} м/с";

            WeatherContent.IsVisible = true;
        }
        catch
        {
            WeatherErrorLabel.Text = "Не удалось загрузить погоду";
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

            var appeals = await _appealService.GetMyAppealsAsync(id);
            var applications = await _applicationService.GetMyApplicationsAsync(id);

            var all = appeals.Count + applications.Count;
            // статус 2 = В обработке, статус 3 = Выполнено
            var active = appeals.Count(a => a.AppealStatusId == 2)
                       + applications.Count(a => a.ApplicationStatusId == 2);
            var done = appeals.Count(a => a.AppealStatusId == 3)
                       + applications.Count(a => a.ApplicationStatusId == 3);

            TotalCountLabel.Text = all.ToString();
            ActiveCountLabel.Text = active.ToString();
            DoneCountLabel.Text = done.ToString();
        }
        catch { /* тихо игнорируем */ }
    }

    // ── Новости RSS ───────────────────────────────────────────
    private async Task LoadNewsAsync()
    {
        try
        {
            NewsLoading.IsVisible = true;
            NewsLoading.IsRunning = true;
            NewsContent.IsVisible = false;
            NewsErrorLabel.IsVisible = false;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

            var xml = await _httpClient.GetStringAsync(AppConstants.NewsRssUrl);
            var doc = XDocument.Parse(xml);

            var items = doc.Descendants("item")
                           .Take(5)
                           .Select(i => new
                           {
                               Title = i.Element("title")?.Value ?? "",
                               Link = i.Element("link")?.Value ?? "",
                               Date = i.Element("pubDate")?.Value ?? ""
                           })
                           .ToList();

            NewsContent.Children.Clear();

            foreach (var item in items)
            {
                // Разделитель
                if (NewsContent.Children.Count > 0)
                    NewsContent.Children.Add(new BoxView
                    {
                        HeightRequest = 1,
                        Color = Color.FromArgb("#E0E0E0"),
                        Margin = new Thickness(0, 8)
                    });

                var titleLabel = new Label
                {
                    Text = item.Title,
                    FontSize = 13,
                    TextColor = Color.FromArgb("#1A237E"),
                    TextDecorations = TextDecorations.Underline
                };

                var link = item.Link; // захватываем для замыкания
                titleLabel.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                        await Launcher.OpenAsync(new Uri(link)))
                });

                var dateLabel = new Label
                {
                    Text = item.Date,
                    FontSize = 11,
                    TextColor = Color.FromArgb("#9E9E9E"),
                    Margin = new Thickness(0, 2, 0, 0)
                };

                NewsContent.Children.Add(titleLabel);
                NewsContent.Children.Add(dateLabel);
            }

            NewsContent.IsVisible = true;
        }
        catch
        {
            NewsErrorLabel.Text = "Не удалось загрузить новости";
            NewsErrorLabel.IsVisible = true;
        }
        finally
        {
            NewsLoading.IsVisible = false;
            NewsLoading.IsRunning = false;
        }
    }

    // ── Обработчики ───────────────────────────────────────────
    private async void OnPhoneTapped(object sender, TappedEventArgs e)
    {
        await Launcher.OpenAsync("tel:+73420000000");
    }

    private async void OnEmailTapped(object sender, TappedEventArgs e)
    {
        await Launcher.OpenAsync("mailto:admin@berezovsky.ru");
    }

    private async void OnRefreshing(object sender, EventArgs e)
    {
        await LoadAllAsync();
        RefreshView.IsRefreshing = false;
    }
}