using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Applications;

[QueryProperty(nameof(ApplicationId), "id")]
public partial class ApplicationDetailPage : ContentPage
{
    private readonly ApplicationService _applicationService;
    private readonly AttachmentService _attachmentService;
    private int _applicationId;

    public int ApplicationId
    {
        get => _applicationId;
        set
        {
            _applicationId = value;
            LoadApplicationAsync(value).ConfigureAwait(false);
        }
    }

    public ApplicationDetailPage(
        ApplicationService applicationService,
        AttachmentService attachmentService)
    {
        InitializeComponent();
        _applicationService = applicationService;
        _attachmentService = attachmentService;
    }

    private async Task LoadApplicationAsync(int id)
    {
        try
        {
            var app = await _applicationService.GetByIdAsync(id);
            if (app is null)
            {
                await DisplayAlert("Ошибка", "Заявка не найдена", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            NumberLabel.Text = app.ApplicationNumber;
            TitleLabel.Text = app.Title;
            DateLabel.Text = $"Подана: {app.CreationDate:dd.MM.yyyy HH:mm}";
            TextLabel.Text = app.DocumentText;
            StatusLabel.Text = GetStatusText(app.ApplicationStatusId);
            StatusFrame.BackgroundColor = GetStatusColor(app.ApplicationStatusId);

            if (!string.IsNullOrEmpty(app.ResponseText))
            {
                ResponseLabel.Text = app.ResponseText;
                ResponseDateLabel.Text = app.ResponseDate.HasValue
                    ? $"Дата ответа: {app.ResponseDate.Value:dd.MM.yyyy}"
                    : string.Empty;
                ResponseFrame.IsVisible = true;
            }

            DeleteButton.IsVisible = app.ApplicationStatusId == 1;
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

    private async Task LoadAttachmentsAsync(int applicationId)
    {
        try
        {
            var attachments = await _attachmentService
                .GetByApplicationAsync(applicationId);
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

                grid.Add(new Label
                {
                    Text = AttachmentHelper.GetIcon(att.FileName),
                    FontSize = 22,
                    VerticalOptions = LayoutOptions.Center
                }, 0, 0);

                grid.Add(new VerticalStackLayout
                {
                    Children =
                    {
                        new Label
                        {
                            Text          = att.FileName,
                            FontSize      = 13,
                            TextColor     = Color.FromArgb("#212121"),
                            LineBreakMode = LineBreakMode.TailTruncation
                        },
                        new Label
                        {
                            Text      = AttachmentHelper.GetSizeText(att.FileSize),
                            FontSize  = 11,
                            TextColor = Color.FromArgb("#9E9E9E")
                        }
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
        catch { }
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

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        var confirm = await DisplayAlert(
            "Удаление", "Вы уверены что хотите удалить эту заявку?",
            "Удалить", "Отмена");
        if (!confirm) return;

        try
        {
            await _applicationService.DeleteAsync(_applicationId);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }

    private static string GetStatusText(int statusId) => statusId switch
    {
        1 => "Новая",
        2 => "В обработке",
        3 => "Выполнена",
        4 => "Отклонена",
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
}