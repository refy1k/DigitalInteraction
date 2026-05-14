using DigitalInteraction.Models;

namespace DigitalInteraction.Helpers;

public static class SessionManager
{
    public static int? CurrentCitizenId { get; set; }
    public static Citizen? CurrentCitizen { get; set; }
    public static bool IsLoggedIn => CurrentCitizenId.HasValue;

    public static void Clear()
    {
        CurrentCitizenId = null;
        CurrentCitizen = null;
    }

    public static string? CurrentAvatarPath { get; set; }
}