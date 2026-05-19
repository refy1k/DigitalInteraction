using System.Net;
using System.Net.Mail;
using DigitalInteraction.Helpers;

namespace DigitalInteraction.Services;

public class EmailService
{
    // Генерация 6-значного кода
    public static string GenerateCode()
    {
        var rng = new Random();
        return rng.Next(100000, 999999).ToString();
    }

    // Отправка кода подтверждения
    public async Task SendVerificationCodeAsync(string toEmail, string code)
    {
        var subject = "Подтверждение электронной почты";
        var body = $"""
            Здравствуйте!

            Ваш код подтверждения электронной почты в системе
            «Цифровое взаимодействие»:

            {code}

            Код действителен в течение 10 минут.

            Если вы не запрашивали подтверждение — проигнорируйте это письмо.

            С уважением,
            Администрация Берёзовского МО
            """;

        await SendAsync(toEmail, subject, body);
    }

    // Отправка нового пароля
    public async Task SendNewPasswordAsync(string toEmail, string newPassword)
    {
        var subject = "Восстановление пароля";
        var body = $"""
            Здравствуйте!

            По вашему запросу был сгенерирован новый пароль
            для входа в систему «Цифровое взаимодействие»:

            {newPassword}

            Рекомендуем сменить пароль после входа в систему.

            С уважением,
            Администрация Берёзовского МО
            """;

        await SendAsync(toEmail, subject, body);
    }

    // Базовый метод отправки
    private async Task SendAsync(string toEmail, string subject, string body)
    {
        using var client = new SmtpClient(AppConstants.SmtpHost, AppConstants.SmtpPort)
        {
            Credentials = new NetworkCredential(
                AppConstants.SmtpEmail,
                AppConstants.SmtpPassword),
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        var message = new MailMessage
        {
            From = new MailAddress(
                AppConstants.SmtpEmail,
                AppConstants.SmtpFromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }
}