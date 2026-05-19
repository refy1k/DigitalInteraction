using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Appeals;

public partial class CreateAppealPage : ContentPage
{
    private readonly AppealService _appealService;
    private readonly AttachmentService _attachmentService;

    // Список файлов для прикрепления
    private readonly List<(string FileName, Stream Stream, long Size)> _pendingFiles = [];

    public CreateAppealPage(AppealService appealService, AttachmentService attachmentService)
    {
        InitializeComponent();
        _appealService = appealService;
        _attachmentService = attachmentService;
    }

    private async void OnAttachFileClicked(object sender, EventArgs e)
    {
        try
        {
            var options = new PickOptions
            {
                PickerTitle = "Выберите файл",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, [ "application/msword",
                        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                        "application/pdf", "text/plain",
                        "application/vnd.ms-excel",
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" ] },
                    { DevicePlatform.WinUI,   [ ".doc", ".docx", ".pdf", ".txt", ".xls", ".xlsx" ] },
                    { DevicePlatform.iOS,     [ "com.microsoft.word.doc", "org.openxmlformats.wordprocessingml.document" ] },
                })
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result is null) return;

            // Проверяем размер (макс 10 МБ)
            var stream = await result.OpenReadAsync();
            if (stream.Length > 10 * 1024 * 1024)
            {
                await DisplayAlert("Ошибка", "Файл слишком большой. Максимум 10 МБ.", "OK");
                return;
            }

            // Проверяем дубликаты
            if (_pendingFiles.Any(f => f.FileName == result.FileName))
            {
                await DisplayAlert("Ошибка", "Этот файл уже прикреплён.", "OK");
                return;
            }

            _pendingFiles.Add((result.FileName, stream, stream.Length));
            RefreshAttachmentsList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private void RefreshAttachmentsList()
    {
        AttachmentsLayout.Children.Clear();
        NoAttachmentsLabel.IsVisible = _pendingFiles.Count == 0;

        foreach (var file in _pendingFiles)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            var icon = ext switch
            {
                ".doc" or ".docx" => "📄",
                ".pdf" => "📕",
                ".xls" or ".xlsx" => "📊",
                ".txt" => "📝",
                _ => "📎"
            };
            var sizeText = file.Size switch
            {
                < 1024 => $"{file.Size} Б",
                < 1024 * 1024 => $"{file.Size / 1024} КБ",
                _ => $"{file.Size / (1024 * 1024)} МБ"
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 8
            };

            grid.Add(new Label { Text = icon, FontSize = 20, VerticalOptions = LayoutOptions.Center }, 0, 0);
            grid.Add(new VerticalStackLayout
            {
                Children =
                {
                    new Label { Text = file.FileName, FontSize = 13, TextColor = Color.FromArgb("#212121"), LineBreakMode = LineBreakMode.TailTruncation },
                    new Label { Text = sizeText,       FontSize = 11, TextColor = Color.FromArgb("#9E9E9E") }
                }
            }, 1, 0);

            var fileName = file.FileName;
            var deleteBtn = new Label
            {
                Text = "✕",
                FontSize = 16,
                TextColor = Color.FromArgb("#B71C1C"),
                VerticalOptions = LayoutOptions.Center
            };
            deleteBtn.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    var item = _pendingFiles.FirstOrDefault(f => f.FileName == fileName);
                    if (item != default)
                    {
                        item.Stream.Dispose();
                        _pendingFiles.Remove(item);
                        RefreshAttachmentsList();
                    }
                })
            });
            grid.Add(deleteBtn, 2, 0);

            AttachmentsLayout.Children.Add(grid);

            if (_pendingFiles.IndexOf(file) < _pendingFiles.Count - 1)
                AttachmentsLayout.Children.Add(new BoxView
                { HeightRequest = 1, Color = Color.FromArgb("#F0F0F0") });
        }
    }

    private async void OnSubmitClicked(object sender, EventArgs e)
    {
        var title = TitleEntry.Text?.Trim();
        var text = TextEditor.Text?.Trim();

        if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(text))
        {
            ErrorLabel.Text = "Заполните все обязательные поля";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (title.Length < 5)
        {
            ErrorLabel.Text = "Тема должна содержать минимум 5 символов";
            ErrorLabel.IsVisible = true;
            return;
        }

        if (text.Length < 20)
        {
            ErrorLabel.Text = "Текст обращения должен содержать минимум 20 символов";
            ErrorLabel.IsVisible = true;
            return;
        }

        try
        {
            SetLoading(true);
            ErrorLabel.IsVisible = false;

            // Создаём обращение
            var appeal = await _appealService.CreateAsync(
                SessionManager.CurrentCitizenId!.Value, title, text);

            // Загружаем вложения
            foreach (var file in _pendingFiles)
            {
                file.Stream.Position = 0;
                await _attachmentService.UploadAsync(
                    SessionManager.CurrentCitizenId!.Value,
                    file.Stream,
                    file.FileName,
                    appealId: appeal.Id);
            }

            await DisplayAlert("Успешно",
                _pendingFiles.Count > 0
                    ? $"Обращение отправлено с {_pendingFiles.Count} файлами!"
                    : "Обращение отправлено!",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = $"Ошибка: {ex.Message}";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            SetLoading(false);
        }
    }

    private void SetLoading(bool isLoading)
    {
        LoadingIndicator.IsVisible = isLoading;
        LoadingIndicator.IsRunning = isLoading;
        SubmitButton.IsEnabled = !isLoading;
    }
}