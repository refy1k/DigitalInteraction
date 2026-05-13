using Supabase;
using DigitalInteraction.Helpers;

namespace DigitalInteraction.Services;

public static class SupabaseService
{
    private static Client? _client;

    public static async Task<Client> GetClientAsync()
    {
        if (_client is not null) return _client;

        var options = new SupabaseOptions
        {
            AutoRefreshToken = false,
            AutoConnectRealtime = false
        };

        _client = new Client(
            AppConstants.SupabaseUrl,
            AppConstants.SupabaseAnonKey,
            options);

        await _client.InitializeAsync();
        return _client;
    }
}