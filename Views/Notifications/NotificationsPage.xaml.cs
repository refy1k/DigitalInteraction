using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using DigitalInteraction.Services;

namespace DigitalInteraction.Views.Notifications;

public partial class NotificationsPage : ContentPage
{
    private readonly NotificationService _notificationService;
    private List<Notification> _notifications = [];

    public NotificationsPage(NotificationService notificationService)
    {
        InitializeComponent();
        _notificationService = notificationService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadNotificationsAsync();
    }

    private async Task LoadNotificationsAsync()
    {
        try
        {
            LoadingIndicator.IsVisible = true;
            LoadingIndicator.IsRunning = true;
            NotificationsCollection.IsVisible = false;
            EmptyView.IsVisible = false;
            MarkAllReadButton.IsVisible = false;

            _notifications = await _notificationService
                .GetMyNotificationsAsync(SessionManager.CurrentCitizenId!.Value);

            if (_notifications.Count == 0)
            {
                EmptyView.IsVisible = true;
                return;
            }

            NotificationsCollection.ItemsSource = _notifications;
            NotificationsCollection.IsVisible = true;

            // Показываем кнопку если есть непрочитанные
            MarkAllReadButton.IsVisible = _notifications.Any(n => !n.IsRead);
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

    private async void OnNotificationTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not Notification notification) return;

        // Отмечаем как прочитанное
        if (!notification.IsRead)
        {
            await _notificationService.MarkAsReadAsync(notification.Id);
            notification.IsRead = true;
            // Обновляем список
            NotificationsCollection.ItemsSource = null;
            NotificationsCollection.ItemsSource = _notifications;
            MarkAllReadButton.IsVisible = _notifications.Any(n => !n.IsRead);
        }

        // Показываем полное сообщение
        await DisplayAlert(notification.Title, notification.Message, "OK");
    }

    private async void OnMarkAllReadClicked(object sender, EventArgs e)
    {
        try
        {
            var unread = _notifications.Where(n => !n.IsRead).ToList();
            foreach (var n in unread)
            {
                await _notificationService.MarkAsReadAsync(n.Id);
                n.IsRead = true;
            }

            NotificationsCollection.ItemsSource = null;
            NotificationsCollection.ItemsSource = _notifications;
            MarkAllReadButton.IsVisible = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", ex.Message, "OK");
        }
    }
}