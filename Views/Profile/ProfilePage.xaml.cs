using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Profile;

public partial class ProfilePage : ContentPage
{
    private readonly AppealService _appealService;
    private readonly ApplicationService _applicationService;
    private readonly AuthService _authService;
    private readonly ProfileService _profileService;
    private readonly MunicipalServiceService _serviceService;

    public ProfilePage(
        AppealService appealService,
        ApplicationService applicationService,
        AuthService authService,
        ProfileService profileService,
        MunicipalServiceService serviceService)
    {
        InitializeComponent();
        _appealService = appealService;
        _applicationService = applicationService;
        _authService = authService;
        _profileService = profileService;
        _serviceService = serviceService;
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
            if (citizen is null)
            {
                LoadingIndicator.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                return;
            }

            // Шапка
            FullNameLabel.Text = $"{citizen.LastName} {citizen.FirstName} {citizen.MiddleName}".Trim();
            LoginLabel.Text = $"@{citizen.Login}";
            MemberSinceLabel.Text = $"В системе с {citizen.CreatedAt:dd.MM.yyyy}";

            // Режим просмотра
            LastNameView.Text = citizen.LastName;
            FirstNameView.Text = citizen.FirstName;
            MiddleNameView.Text = string.IsNullOrEmpty(citizen.MiddleName)
                ? "Не указано" : citizen.MiddleName;
            BirthDateView.Text = citizen.DateOfBirth.HasValue
                ? $"{citizen.DateOfBirth.Value:dd.MM.yyyy}"
                : "Не указана";

            // Режим редактирования
            LastNameEntry.Text = citizen.LastName;
            FirstNameEntry.Text = citizen.FirstName;
            MiddleNameEntry.Text = citizen.MiddleName ?? string.Empty;
            BirthDatePicker.Date = citizen.DateOfBirth ?? new DateTime(1990, 1, 1);

            // Показываем контент сразу — не ждём загрузки всего
            ContentLayout.IsVisible = true;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            // Аватар и статистику грузим после — не блокируем UI
            _ = LoadAvatarAsync(citizen.Id);
            _ = LoadStatsAsync(citizen.Id);
        }
        catch (Exception ex)
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private async Task LoadStatsAsync(int citizenId)
    {
        try
        {
            var appeals = await _appealService.GetMyAppealsAsync(citizenId);
            var applications = await _applicationService.GetMyApplicationsAsync(citizenId);
            var services = await _serviceService.GetMyRequestsAsync(citizenId);

            AppealsCountLabel.Text = appeals.Count.ToString();
            ApplicationsCountLabel.Text = applications.Count.ToString();
            ServicesCountLabel.Text = services.Count.ToString();
        }
        catch
        {
            AppealsCountLabel.Text = "—";
            ApplicationsCountLabel.Text = "—";
            ServicesCountLabel.Text = "—";
        }
    }

    // ── Переключение режимов ──────────────────────────────────
    private void OnEditModeTapped(object sender, TappedEventArgs e)
    {
        ViewModeLayout.IsVisible = false;
        EditModeLayout.IsVisible = true;
    }

    private void OnCancelEditTapped(object sender, TappedEventArgs e)
    {
        PersonalInfoError.IsVisible = false;
        PersonalInfoSuccess.IsVisible = false;
        ViewModeLayout.IsVisible = true;
        EditModeLayout.IsVisible = false;
    }

    // ── Сворачивание пароля ───────────────────────────────────
    private void OnTogglePasswordSection(object sender, TappedEventArgs e)
    {
        PasswordSection.IsVisible = !PasswordSection.IsVisible;
        PasswordToggleLabel.Text = PasswordSection.IsVisible ? "Скрыть" : "Показать";
    }

    // ── Сохранение личных данных ──────────────────────────────
    private async void OnSavePersonalClicked(object sender, EventArgs e)
    {
        var lastName = LastNameEntry.Text?.Trim();
        var firstName = FirstNameEntry.Text?.Trim();
        var middleName = MiddleNameEntry.Text?.Trim();

        if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
        {
            PersonalInfoError.Text = "Фамилия и имя обязательны";
            PersonalInfoError.IsVisible = true;
            return;
        }

        try
        {
            SavePersonalButton.IsEnabled = false;
            PersonalInfoError.IsVisible = false;
            PersonalInfoSuccess.IsVisible = false;

            var dateOfBirth = BirthDatePicker.Date;

            await _profileService.UpdatePersonalInfoAsync(
                SessionManager.CurrentCitizenId!.Value,
                lastName, firstName,
                string.IsNullOrEmpty(middleName) ? null : middleName,
                dateOfBirth);

            // Обновляем Labels в режиме просмотра
            LastNameView.Text = lastName;
            FirstNameView.Text = firstName;
            MiddleNameView.Text = string.IsNullOrEmpty(middleName) ? "Не указано" : middleName;
            BirthDateView.Text = $"{dateOfBirth:dd.MM.yyyy}";
            FullNameLabel.Text = $"{lastName} {firstName} {middleName}".Trim();

            PersonalInfoSuccess.Text = "✅ Данные сохранены";
            PersonalInfoSuccess.IsVisible = true;

            // Через 1.5 секунды возвращаемся в режим просмотра
            await Task.Delay(1500);
            ViewModeLayout.IsVisible = true;
            EditModeLayout.IsVisible = false;
        }
        catch (Exception ex)
        {
            PersonalInfoError.Text = $"Ошибка: {ex.Message}";
            PersonalInfoError.IsVisible = true;
        }
        finally
        {
            SavePersonalButton.IsEnabled = true;
        }
    }

    // ── Смена пароля ──────────────────────────────────────────
    private async void OnChangePasswordClicked(object sender, EventArgs e)
    {
        var current = CurrentPasswordEntry.Text;
        var newPass = NewPasswordEntry.Text;
        var confirm = ConfirmPasswordEntry.Text;

        PasswordError.IsVisible = false;
        PasswordSuccess.IsVisible = false;

        if (string.IsNullOrEmpty(current) ||
            string.IsNullOrEmpty(newPass) ||
            string.IsNullOrEmpty(confirm))
        {
            PasswordError.Text = "Заполните все поля";
            PasswordError.IsVisible = true;
            return;
        }

        if (newPass.Length < 6)
        {
            PasswordError.Text = "Новый пароль — минимум 6 символов";
            PasswordError.IsVisible = true;
            return;
        }

        if (newPass != confirm)
        {
            PasswordError.Text = "Пароли не совпадают";
            PasswordError.IsVisible = true;
            return;
        }

        try
        {
            ChangePasswordButton.IsEnabled = false;

            var success = await _profileService.ChangePasswordAsync(
                SessionManager.CurrentCitizenId!.Value,
                current, newPass);

            if (!success)
            {
                PasswordError.Text = "Неверный текущий пароль";
                PasswordError.IsVisible = true;
                return;
            }

            CurrentPasswordEntry.Text = string.Empty;
            NewPasswordEntry.Text = string.Empty;
            ConfirmPasswordEntry.Text = string.Empty;

            PasswordSuccess.Text = "✅ Пароль успешно изменён";
            PasswordSuccess.IsVisible = true;

            await Task.Delay(1500);
            PasswordSection.IsVisible = false;
            PasswordToggleLabel.Text = "Показать";
            PasswordSuccess.IsVisible = false;
        }
        catch (Exception ex)
        {
            PasswordError.Text = $"Ошибка: {ex.Message}";
            PasswordError.IsVisible = true;
        }
        finally
        {
            ChangePasswordButton.IsEnabled = true;
        }
    }

    // ── Аватар ────────────────────────────────────────────────
    private async Task LoadAvatarAsync(int citizenId)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AvatarImage.Source = null;
                AvatarFrame.IsVisible = false;
                AvatarPlaceholder.IsVisible = true;
            });

            // Всегда ищем через Storage API — не полагаемся на SessionManager
            var files = await _profileService.FindLatestAvatarUrlAsync(citizenId);

            if (string.IsNullOrEmpty(files)) return;

            using var http = new System.Net.Http.HttpClient();
            // Добавляем timestamp чтобы избежать кеша
            var urlWithTs = $"{files}?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            var response = await http.GetAsync(urlWithTs);

            if (!response.IsSuccessStatusCode) return;

            var bytes = await response.Content.ReadAsByteArrayAsync();

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AvatarImage.Source = null; // сбрасываем перед установкой
                AvatarImage.Source = ImageSource.FromStream(
                    () => new MemoryStream(bytes));
                AvatarFrame.IsVisible = true;
                AvatarPlaceholder.IsVisible = false;
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Avatar load error: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                AvatarFrame.IsVisible = false;
                AvatarPlaceholder.IsVisible = true;
            });
        }

    }

    private async void OnChangeAvatarTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var result = await MediaPicker.Default.PickPhotoAsync(
                new MediaPickerOptions { Title = "Выберите фото" });

            if (result is null) return;

            var citizenId = SessionManager.CurrentCitizenId!.Value;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                AvatarImage.Source = null;
                AvatarFrame.IsVisible = false;
                AvatarPlaceholder.IsVisible = true;
            });

            using var stream = await result.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            await _profileService.UploadAvatarAsync(
                citizenId, memoryStream, result.FileName);

            await LoadAvatarAsync(citizenId);
            await DisplayAlert("Готово", "Фото обновлено!", "OK");
            await _profileService.UploadAvatarAsync(
    citizenId, memoryStream, result.FileName);

            // Пауза чтобы Supabase успел обработать файл
            await Task.Delay(800);

            await LoadAvatarAsync(citizenId);
            await DisplayAlert("Готово", "Фото обновлено!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    // ── Навигация ─────────────────────────────────────────────
    private async void OnDocumentsTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(nameof(DocumentsPage));

    private async void OnContactInfoTapped(object sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(nameof(ContactInfoPage));

    private async void OnLogoutTapped(object sender, TappedEventArgs e)
    {
        var confirm = await DisplayAlert(
            "Выход", "Вы уверены что хотите выйти?",
            "Выйти", "Отмена");

        if (!confirm) return;

        _authService.Logout();
        await Shell.Current.GoToAsync("//auth/login");
    }
}