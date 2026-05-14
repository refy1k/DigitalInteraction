using Supabase;
using DigitalInteraction.Models;
using DigitalInteraction.Helpers;

namespace DigitalInteraction.Services;

public class ProfileService(Client client)
{
    public async Task<CitizenDocument?> GetDocumentsAsync(int citizenId) =>
        await client.From<CitizenDocument>()
            .Where(d => d.CitizenId == citizenId)
            .Single();

    public async Task SaveDocumentsAsync(CitizenDocument doc) =>
        await client.From<CitizenDocument>().Upsert(doc);

    public async Task<ContactInfo?> GetContactInfoAsync(int citizenId) =>
        await client.From<ContactInfo>()
            .Where(c => c.CitizenId == citizenId)
            .Single();

    public async Task SaveContactInfoAsync(ContactInfo info) =>
        await client.From<ContactInfo>().Upsert(info);

    // Обновление личных данных
    public async Task UpdatePersonalInfoAsync(
        int citizenId,
        string lastName, string firstName, string? middleName,
        DateTime? dateOfBirth)
    {
        await client.From<Citizen>()
            .Where(c => c.Id == citizenId)
            .Set(c => c.LastName, lastName)
            .Set(c => c.FirstName, firstName)
            .Set(c => c.MiddleName, middleName)
            .Set(c => c.DateOfBirth, dateOfBirth)
            .Update();

        // Обновляем в SessionManager
        if (SessionManager.CurrentCitizen is not null)
        {
            SessionManager.CurrentCitizen.LastName = lastName;
            SessionManager.CurrentCitizen.FirstName = firstName;
            SessionManager.CurrentCitizen.MiddleName = middleName;
            SessionManager.CurrentCitizen.DateOfBirth = dateOfBirth;
        }
    }

    // Смена пароля
    public async Task<bool> ChangePasswordAsync(
        int citizenId, string currentPassword, string newPassword)
    {
        var citizen = await client.From<Citizen>()
            .Where(c => c.Id == citizenId)
            .Single();

        if (citizen is null) return false;

        // Проверяем текущий пароль
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, citizen.PasswordHash))
            return false;

        // Сохраняем новый
        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await client.From<Citizen>()
            .Where(c => c.Id == citizenId)
            .Set(c => c.PasswordHash, newHash)
            .Update();

        return true;
    }

    public async Task<string?> UploadAvatarAsync(
        int citizenId, Stream imageStream, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLower();
        if (string.IsNullOrEmpty(ext)) ext = ".jpg";

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var storagePath = $"{citizenId}/avatar_{timestamp}{ext}";

        byte[] bytes;
        if (imageStream is MemoryStream ms)
            bytes = ms.ToArray();
        else
        {
            using var copy = new MemoryStream();
            await imageStream.CopyToAsync(copy);
            bytes = copy.ToArray();
        }

        await client.Storage
            .From("avatars")
            .Upload(bytes, storagePath, new Supabase.Storage.FileOptions
            {
                Upsert = true,
                ContentType = ext == ".png" ? "image/png" : "image/jpeg"
            });

        SessionManager.CurrentAvatarPath = storagePath;

        return client.Storage.From("avatars").GetPublicUrl(storagePath);
    }

    public string? GetAvatarUrl()
    {
        if (string.IsNullOrEmpty(SessionManager.CurrentAvatarPath))
            return null;
        return client.Storage
            .From("avatars")
            .GetPublicUrl(SessionManager.CurrentAvatarPath);
    }

    public async Task<string?> FindLatestAvatarUrlAsync(int citizenId)
    {
        try
        {
            var files = await client.Storage
                .From("avatars")
                .List(citizenId.ToString());

            if (files is null || files.Count == 0) return null;

            var latest = files
                .Where(f => f.Name.StartsWith("avatar_"))
                .OrderByDescending(f => f.Name)
                .FirstOrDefault();

            if (latest is null) return null;

            return client.Storage
                .From("avatars")
                .GetPublicUrl($"{citizenId}/{latest.Name}");
        }
        catch { return null; }
    }
}