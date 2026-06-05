using Supabase;
using DigitalInteraction.Helpers;
using DigitalInteraction.Models;

namespace DigitalInteraction.Services;

public class AuthService(Client client)
{
    public async Task<Citizen?> LoginAsync(string login, string password)
    {
        var result = await client
            .From<Citizen>()
            .Where(c => c.Login == login)
            .Single();

        if (result is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, result.PasswordHash)) return null;

        SessionManager.CurrentCitizenId = result.Id;
        SessionManager.CurrentCitizen = result;
        return result;
    }

    public async Task<Citizen> RegisterAsync(
        string lastName, string firstName, string? middleName,
        string login, string password)
    {
        var citizen = new Citizen
        {
            LastName = lastName,
            FirstName = firstName,
            MiddleName = middleName,
            Login = login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        var response = await client
            .From<Citizen>()
            .Select("id, last_name, first_name, middle_name, login, password_hash, created_at")
            .Insert(citizen);
        var created = response.Models.First();

        // Создаём пустые связанные записи
        await client.From<CitizenDocument>()
            .Insert(new CitizenDocument { CitizenId = created.Id });
        await client.From<ContactInfo>()
            .Insert(new ContactInfo { CitizenId = created.Id });

        SessionManager.CurrentCitizenId = created.Id;
        SessionManager.CurrentCitizen = created;
        return created;
    }

    public void Logout() => SessionManager.Clear();
}