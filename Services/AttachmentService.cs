using Supabase;
using DigitalInteraction.Helpers;
using DigitalInteraction.Models;
using System.Net.Http.Headers;

namespace DigitalInteraction.Services;

public class AttachmentService(Client client)
{
    private const string BucketName = "documents";

    public async Task<Attachment> UploadAsync(
        int citizenId,
        Stream fileStream,
        string fileName,
        int? appealId = null,
        int? applicationId = null)
    {
        var ext = Path.GetExtension(fileName);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var storagePath = $"{citizenId}/{timestamp}_{fileName}";

        byte[] bytes;
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        bytes = ms.ToArray();

        await client.Storage
            .From(BucketName)
            .Upload(bytes, storagePath, new Supabase.Storage.FileOptions
            {
                Upsert = false,
                ContentType = GetContentType(ext)
            });

        var attachment = new Attachment
        {
            CitizenId = citizenId,
            AppealId = appealId,
            ApplicationId = applicationId,
            FileName = fileName,
            FilePath = storagePath,
            FileSize = bytes.Length,
            CreatedAt = DateTime.UtcNow
        };

        var response = await client.From<Attachment>().Insert(attachment);
        return response.Models.First();
    }

    public async Task<List<Attachment>> GetByAppealAsync(int appealId)
    {
        var result = await client.From<Attachment>()
            .Where(a => a.AppealId == appealId)
            .Get();
        return result.Models;
    }

    public async Task<List<Attachment>> GetByApplicationAsync(int applicationId)
    {
        var result = await client.From<Attachment>()
            .Where(a => a.ApplicationId == applicationId)
            .Get();
        return result.Models;
    }

    // Скачиваем напрямую через HTTP REST API Supabase Storage
    public async Task<byte[]> DownloadAsync(string filePath)
    {
        // URL формата: {SupabaseUrl}/storage/v1/object/public/{bucket}/{path}
        // для публичного bucket, или с anon key для приватного
        var url = $"{AppConstants.SupabaseUrl}/storage/v1/object/{BucketName}/{filePath}";

        using var http = new System.Net.Http.HttpClient();

        // Добавляем anon key для авторизации
        http.DefaultRequestHeaders.Add("apikey", AppConstants.SupabaseAnonKey);
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", AppConstants.SupabaseAnonKey);

        var response = await http.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Ошибка загрузки файла: {response.StatusCode}");

        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task DeleteAsync(int attachmentId, string filePath)
    {
        await client.Storage
            .From(BucketName)
            .Remove([filePath]);

        await client.From<Attachment>()
            .Where(a => a.Id == attachmentId)
            .Delete();
    }

    private static string GetContentType(string ext) => ext.ToLower() switch
    {
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".pdf" => "application/pdf",
        ".xls" => "application/vnd.ms-excel",
        ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        ".txt" => "text/plain",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        _ => "application/octet-stream"
    };
}