using DigitalInteraction.Services;

using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Appeals;

[QueryProperty(nameof(AppealId), "id")]
public partial class AppealDetailPage : ContentPage
{
    private readonly AppealService _appealService;
    private readonly AttachmentService _attachmentService;
    private int _appealId;

    public int AppealId
    {
        get => _appealId;
        set
        {
            _appealId = value;
            LoadAppealAsync(value).ConfigureAwait(false);
        }
    }

    public AppealDetailPage(AppealService appealService, AttachmentService attachmentService)
    {
        InitializeComponent();
        _appealService = appealService;
        _attachmentService = attachmentService;
    }

    private async Task LoadAppealAsync(int id)
    {
        try
        {
            var appeal = await _appealService.GetByIdAsync(id);
            if (appeal is null)
            {
                await DisplayAlert("Ошибка", "Обращение не найдено", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Заполняем данные
            NumberLabel.Text = appeal.AppealNumber;
            TitleLabel.Text = appeal.Title;
            DateLabel.Text = $"Подано: {appeal.CreationDate:dd.MM.yyyy HH:mm}";
            TextLabel.Text = appeal.DocumentText;

            // Статус
            StatusLabel.Text = GetStatusText(appeal.AppealStatusId);
            StatusFrame.BackgroundColor = GetStatusColor(appeal.AppealStatusId);

            // Ответ
            if (!string.IsNullOrEmpty(appeal.ResponseText))
            {
                ResponseLabel.Text = appeal.ResponseText;
                ResponseDateLabel.Text = appeal.ResponseDate.HasValue
                    ? $"Дата ответа: {appeal.ResponseDate.Value:dd.MM.yyyy}"
                    : string.Empty;
                ResponseFrame.IsVisible = true;
            }

            // Кнопка удалить — только для статуса «Новое»
            DeleteButton.IsVisible = appeal.AppealStatusId == 1;

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

        await LoadAttachmentsAsync(id);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Удаление",
            "Вы уверены что хотите удалить это обращение?",
            "Удалить", "Отмена");

        if (!confirm) return;

        try
        {
            await _appealService.DeleteAsync(_appealId);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private static string GetStatusText(int statusId) => statusId switch
    {
        1 => "Новое",
        2 => "В обработке",
        3 => "Выполнено",
        4 => "Отклонено",
        _ => "Неизвестно"
    };

    private static Color GetStatusColor(int statusId) => statusId switch
    {
        1 => Color.FromArgb("#1565C0"),
        2 => Color.FromArgb("#E65100"),
        3 => Color.FromArgb("#2E7D32"),
        4 => Color.FromArgb("#B71C1C"),
        _ => Color.FromArgb("#757575")
    };

    private async Task LoadAttachmentsAsync(int appealId)
    {
        try
        {
            var attachments = await _attachmentService.GetByAppealAsync(appealId);
            if (attachments.Count == 0) return;

            AttachmentsDetailLayout.Children.Clear();

            foreach (var att in attachments)
            {
                var grid = new Grid
                {
                    ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                    ColumnSpacing = 10
                };

                grid.Add(new Label { Text = att.FileIcon, FontSize = 22, VerticalOptions = LayoutOptions.Center }, 0, 0);
                grid.Add(new VerticalStackLayout
                {
                    Children =
                {
                    new Label { Text = att.FileName,     FontSize = 13, TextColor = Color.FromArgb("#212121"), LineBreakMode = LineBreakMode.TailTruncation },
                    new Label { Text = att.FileSizeText, FontSize = 11, TextColor = Color.FromArgb("#9E9E9E") }
                }
                }, 1, 0);

                var filePath = att.FilePath;
                var fileName = att.FileName;
                var downloadBtn = new Label
                {
                    Text = "⬇️",
                    FontSize = 20,
                    VerticalOptions = LayoutOptions.Center
                };
                downloadBtn.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(async () =>
                        await DownloadAndOpenFileAsync(filePath, fileName))
                });
                grid.Add(downloadBtn, 2, 0);

                AttachmentsDetailLayout.Children.Add(grid);
            }

            AttachmentsFrame.IsVisible = true;
        }
        catch { /* тихо игнорируем */ }
    }

    private async Task DownloadAndOpenFileAsync(string filePath, string fileName)
    {
        try
        {
            var bytes = await _attachmentService.DownloadAsync(filePath);
            var tempPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            await File.WriteAllBytesAsync(tempPath, bytes);
            await Launcher.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(tempPath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось открыть файл: {ex.Message}", "OK");
        }
    }
}