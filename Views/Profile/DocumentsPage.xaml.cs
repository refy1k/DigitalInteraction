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

            if (_document is not null)
            {
                PassportEntry.Text = _document.PassportNumber ?? string.Empty;
                SnilsEntry.Text = _document.SnilsNumber ?? string.Empty;
                InnEntry.Text = _document.InnNumber ?? string.Empty;
                OmsEntry.Text = _document.OmsPolicyNumber ?? string.Empty;
            }

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

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            SetSaving(true);
            ErrorLabel.IsVisible = false;
            SuccessLabel.IsVisible = false;

            var doc = new CitizenDocument
            {
                Id = _document?.Id ?? 0,
                CitizenId = SessionManager.CurrentCitizenId!.Value,
                PassportNumber = PassportEntry.Text?.Trim(),
                SnilsNumber = SnilsEntry.Text?.Trim(),
                InnNumber = InnEntry.Text?.Trim(),
                OmsPolicyNumber = OmsEntry.Text?.Trim()
            };

            await _profileService.SaveDocumentsAsync(doc);

            SuccessLabel.Text = "✅ Документы сохранены";
            SuccessLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Ошибка: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            SetSaving(false);
        }
    }

    private void SetSaving(bool isSaving)
    {
        SavingIndicator.IsVisible = isSaving;
        SavingIndicator.IsRunning = isSaving;
        SaveButton.IsEnabled = !isSaving;
    }
}