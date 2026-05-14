using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Services;

public partial class MunicipalServicesPage : ContentPage
{
    private readonly MunicipalServiceService _serviceService;
    private List<ServiceCategory> _categories = [];
    private List<MunicipalService> _allServices = [];

    public MunicipalServicesPage(MunicipalServiceService serviceService)
    {
        InitializeComponent();
        _serviceService = serviceService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            CategoriesLayout.IsVisible = false;

            _categories = await _serviceService.GetCategoriesAsync();

            // Загружаем все услуги для поиска
            foreach (var cat in _categories)
            {
                var services = await _serviceService
                    .GetServicesByCategoryAsync(cat.Id);
                _allServices.AddRange(services);
            }

            BuildCategoryCards();
            CategoriesLayout.IsVisible = true;
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

    private void BuildCategoryCards()
    {
        CategoriesLayout.Children.Clear();

        // Строим сетку 2 колонки
        Grid? currentRow = null;
        for (int i = 0; i < _categories.Count; i++)
        {
            var cat = _categories[i];

            if (i % 2 == 0)
            {
                currentRow = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star }
                    },
                    ColumnSpacing = 12
                };
                CategoriesLayout.Children.Add(currentRow);
            }

            var card = BuildCategoryCard(cat);
            currentRow!.Add(card, i % 2, 0);
        }

        // Если нечётное кол-во — добавляем пустой блок
        if (_categories.Count % 2 != 0 && currentRow is not null)
        {
            currentRow.Add(new BoxView { Color = Colors.Transparent }, 1, 0);
        }
    }

    private Border BuildCategoryCard(ServiceCategory cat)
    {
        var color = Color.FromArgb(cat.Color);

        var border = new Border
        {
            BackgroundColor = color,
            StrokeThickness = 0,
            HeightRequest = 110,
            Padding = new Thickness(14)
        };
        border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
        { CornerRadius = 14 };

        var content = new VerticalStackLayout { Spacing = 6 };

        content.Children.Add(new Label
        {
            Text = cat.Icon,
            FontSize = 32
        });
        content.Children.Add(new Label
        {
            Text = cat.Name,
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White,
            LineBreakMode = LineBreakMode.WordWrap
        });

        border.Content = content;

        border.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
                await GoToCategoryAsync(cat))
        });

        return border;
    }

    private async Task GoToCategoryAsync(ServiceCategory cat)
    {
        await Shell.Current.GoToAsync(
            $"{nameof(MunicipalServiceDetailPage)}?categoryId={cat.Id}&categoryName={Uri.EscapeDataString(cat.Name)}");
    }

    private void OnSearchChanged(object sender, TextChangedEventArgs e)
    {
        var query = e.NewTextValue?.Trim().ToLower();

        if (string.IsNullOrEmpty(query))
        {
            CategoriesLayout.IsVisible = true;
            SearchResultsLayout.IsVisible = false;
            return;
        }

        CategoriesLayout.IsVisible = false;
        SearchResultsLayout.IsVisible = true;
        SearchResultsLayout.Children.Clear();

        var results = _allServices
            .Where(s => s.Name.ToLower().Contains(query) ||
                        s.Description.ToLower().Contains(query))
            .ToList();

        if (results.Count == 0)
        {
            SearchResultsLayout.Children.Add(new Label
            {
                Text = "Ничего не найдено",
                FontSize = 15,
                TextColor = Color.FromArgb("#757575"),
                HorizontalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 40, 0, 0)
            });
            return;
        }

        foreach (var service in results)
            SearchResultsLayout.Children.Add(BuildServiceCard(service));
    }

    private Border BuildServiceCard(MunicipalService service)
    {
        var border = new Border
        {
            BackgroundColor = Colors.White,
            StrokeThickness = 0,
            Padding = new Thickness(16, 12)
        };
        border.StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
        { CornerRadius = 10 };

        var content = new VerticalStackLayout { Spacing = 4 };
        content.Children.Add(new Label
        {
            Text = service.Name,
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#212121")
        });
        content.Children.Add(new Label
        {
            Text = service.Description,
            FontSize = 13,
            TextColor = Color.FromArgb("#757575"),
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 2
        });
        content.Children.Add(new Label
        {
            Text = $"⏱ Срок: {service.DurationDays} дней",
            FontSize = 12,
            TextColor = Color.FromArgb("#9E9E9E")
        });

        border.Content = content;
        border.GestureRecognizers.Add(new TapGestureRecognizer
        {
            Command = new Command(async () =>
                await Shell.Current.GoToAsync(
                    $"{nameof(MunicipalServiceDetailPage)}?serviceId={service.Id}"))
        });

        return border;
    }
}