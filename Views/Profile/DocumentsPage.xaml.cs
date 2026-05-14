using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Profile;

public partial class DocumentsPage : ContentPage
{
    private readonly ProfileService _profileService;
    private CitizenDocument? _document;

    public DocumentsPage(ProfileService profileService)
    {
        InitializeComponent();
        _profileService = profileService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadDocumentsAsync();
    }

    private async Task LoadDocumentsAsync()
    {
        try
        {
            _document = await _profileService
                .GetDocumentsAsync(SessionManager.CurrentCitizenId!.Value);

            PassportNumberLabel.Text = string.IsNullOrEmpty(_document?.PassportNumber)
                ? "Не указан" : _document.PassportNumber;
            SnilsNumberLabel.Text = string.IsNullOrEmpty(_document?.SnilsNumber)
                ? "Не указан" : _document.SnilsNumber;
            InnNumberLabel.Text = string.IsNullOrEmpty(_document?.InnNumber)
                ? "Не указан" : _document.InnNumber;
            OmsNumberLabel.Text = string.IsNullOrEmpty(_document?.OmsPolicyNumber)
                ? "Не указан" : _document.OmsPolicyNumber;

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

    // Тап по карточке — показываем номер крупно
    private async void OnPassportTapped(object sender, TappedEventArgs e) =>
        await ShowDocumentAsync("Паспорт РФ", _document?.PassportNumber);

    private async void OnSnilsTapped(object sender, TappedEventArgs e) =>
        await ShowDocumentAsync("СНИЛС", _document?.SnilsNumber);

    private async void OnInnTapped(object sender, TappedEventArgs e) =>
        await ShowDocumentAsync("ИНН", _document?.InnNumber);

    private async void OnOmsTapped(object sender, TappedEventArgs e) =>
        await ShowDocumentAsync("Полис ОМС", _document?.OmsPolicyNumber);

    private async Task ShowDocumentAsync(string title, string? number)
    {
        if (string.IsNullOrEmpty(number) || number == "Не указан")
        {
            await DisplayAlert(title, "Номер не указан.\nНажмите «Редактировать» чтобы добавить.", "OK");
            return;
        }
        await DisplayAlert(title, number, "OK");
    }

    private async void OnEditClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new DocumentsEditPage(_profileService, _document));
    }
}