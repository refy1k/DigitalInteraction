using DigitalInteraction.Helpers;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Profile;

public partial class ProfilePage : ContentPage
{
    private readonly AppealService _appealService;
    private readonly ApplicationService _applicationService;
    private readonly AuthService _authService;

    public ProfilePage(
        AppealService appealService,
        ApplicationService applicationService,
        AuthService authService)
    {
        InitializeComponent();
        _appealService = appealService;
        _applicationService = applicationService;
        _authService = authService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            var citizen = SessionManager.CurrentCitizen;
            if (citizen is null) return;

            // Данные профиля
            FullNameLabel.Text = $"{citizen.LastName} {citizen.FirstName} {citizen.MiddleName}".Trim();
            LoginLabel.Text = $"@{citizen.Login}";
            MemberSinceLabel.Text = $"В системе с {citizen.CreatedAt:dd.MM.yyyy}";

            // Статистика
            var appeals = await _appealService.GetMyAppealsAsync(citizen.Id);
            var applications = await _applicationService.GetMyApplicationsAsync(citizen.Id);

            AppealsCountLabel.Text = appeals.Count.ToString();
            ApplicationsCountLabel.Text = applications.Count.ToString();

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

    private async void OnDocumentsTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(DocumentsPage));
    }

    private async void OnContactInfoTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ContactInfoPage));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Выход",
            "Вы уверены что хотите выйти?",
            "Выйти", "Отмена");

        if (!confirm) return;

        _authService.Logout();
        await Shell.Current.GoToAsync("//auth/login");
    }
}