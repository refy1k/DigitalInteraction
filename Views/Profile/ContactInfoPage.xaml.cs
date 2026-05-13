using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Profile;

public partial class ContactInfoPage : ContentPage
{
    private readonly ProfileService _profileService;
    private ContactInfo? _contactInfo;

    public ContactInfoPage(ProfileService profileService)
    {
        InitializeComponent();
        _profileService = profileService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadContactInfoAsync();
    }

    private async Task LoadContactInfoAsync()
    {
        try
        {
            _contactInfo = await _profileService
                .GetContactInfoAsync(SessionManager.CurrentCitizenId!.Value);

            if (_contactInfo is not null)
            {
                PhoneEntry.Text = _contactInfo.Phone ?? string.Empty;
                EmailEntry.Text = _contactInfo.Email ?? string.Empty;
                AddressEditor.Text = _contactInfo.CurrentResidence ?? string.Empty;
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

            var info = new ContactInfo
            {
                Id = _contactInfo?.Id ?? 0,
                CitizenId = SessionManager.CurrentCitizenId!.Value,
                Phone = PhoneEntry.Text?.Trim(),
                Email = EmailEntry.Text?.Trim(),
                CurrentResidence = AddressEditor.Text?.Trim()
            };

            await _profileService.SaveContactInfoAsync(info);

            SuccessLabel.Text = "✅ Контактная информация сохранена";
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