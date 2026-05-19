using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using Supabase;

namespace DigitalInteraction.Services;

public class VerificationService(Client client)
{
    private readonly EmailService _emailService = new();

    public async Task SendVerificationCodeAsync(int citizenId, string email)
    {
        // Получаем старые коды и фильтруем на клиенте
        var old = await client.From<EmailVerification>()
            .Where(v => v.CitizenId == citizenId)
            .Get();

        foreach (var v in old.Models.Where(x => !x.IsUsed))
        {
            await client.From<EmailVerification>()
                .Where(x => x.Id == v.Id)
                .Set(x => x.IsUsed, true)
                .Update();
        }

        var code = EmailService.GenerateCode();
        var verification = new EmailVerification
        {
            CitizenId = citizenId,
            Email = email,
            Code = code,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false
        };

        await client.From<EmailVerification>().Insert(verification);
        await _emailService.SendVerificationCodeAsync(email, code);
    }

    public async Task<bool> VerifyCodeAsync(int citizenId, string email, string code)
    {
        // Получаем все коды для этого гражданина и фильтруем на клиенте
        var result = await client.From<EmailVerification>()
            .Where(v => v.CitizenId == citizenId)
            .Get();

        var verification = result.Models
            .FirstOrDefault(v => v.Email == email &&
                                 v.Code == code &&
                                 !v.IsUsed);

        if (verification is null) return false;
        if (DateTime.UtcNow > verification.ExpiresAt) return false;

        await client.From<EmailVerification>()
            .Where(v => v.Id == verification.Id)
            .Set(v => v.IsUsed, true)
            .Update();

        await client.From<Citizen>()
            .Where(c => c.Id == citizenId)
            .Set(c => c.IsEmailVerified, true)
            .Update();

        var contact = await client.From<ContactInfo>()
            .Where(c => c.CitizenId == citizenId)
            .Single();

        if (contact is not null)
        {
            await client.From<ContactInfo>()
                .Where(c => c.Id == contact.Id)
                .Set(c => c.Email, email)
                .Update();
        }

        if (SessionManager.CurrentCitizen is not null)
            SessionManager.CurrentCitizen.IsEmailVerified = true;

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email)
    {
        var contactResult = await client.From<ContactInfo>()
            .Get();

        // Фильтруем на клиенте
        var contact = contactResult.Models
            .FirstOrDefault(c => c.Email == email);

        if (contact is null) return false;

        var newPassword = GeneratePassword();
        var newHash = BCrypt.Net.BCrypt.HashPassword(newPassword);

        await client.From<Citizen>()
            .Where(c => c.Id == contact.CitizenId)
            .Set(c => c.PasswordHash, newHash)
            .Update();

        await _emailService.SendNewPasswordAsync(email, newPassword);
        return true;
    }

    private static string GeneratePassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
        var rng = new Random();
        return new string(Enumerable.Range(0, 10)
            .Select(_ => chars[rng.Next(chars.Length)])
            .ToArray());
    }
}