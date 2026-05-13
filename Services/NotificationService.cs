using Supabase;
using DigitalInteraction.Models;

namespace DigitalInteraction.Services;

public class NotificationService(Client client)
{
    public async Task<List<Notification>> GetMyNotificationsAsync(int citizenId)
    {
        var result = await client
            .From<Notification>()
            .Where(n => n.CitizenId == citizenId)
            .Order("created_at", Postgrest.Constants.Ordering.Descending)
            .Get();
        return result.Models;
    }

    public async Task<int> GetUnreadCountAsync(int citizenId)
    {
        var result = await client
            .From<Notification>()
            .Where(n => n.CitizenId == citizenId && !n.IsRead)
            .Get();
        return result.Models.Count;
    }

    public async Task MarkAsReadAsync(int notificationId)
    {
        await client.From<Notification>()
            .Where(n => n.Id == notificationId)
            .Set(n => n.IsRead, true)
            .Update();
    }
}