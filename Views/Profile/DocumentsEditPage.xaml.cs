using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Profile;

public partial class DocumentsEditPage : ContentPage
{
    private readonly ProfileService _profileService;
    private readonly CitizenDocument? _document;

    public DocumentsEditPage(ProfileService profileService, CitizenDocument? document)
    {
        InitializeComponent();
        _profileService = profileService;
        _document = document;

        // Заполняем существующими данными
        PassportEntry.Text = document?.PassportNumber ?? string.Empty;
        SnilsEntry.Text = document?.SnilsNumber ?? string.Empty;
        InnEntry.Text = document?.InnNumber ?? string.Empty;
        OmsEntry.Text = document?.OmsPolicyNumber ?? string.Empty;
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

            await Task.Delay(1000);
            await Navigation.PopAsync();
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