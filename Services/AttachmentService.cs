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

        // Очищаем имя файла — убираем кириллицу, пробелы и спецсимволы
        var safeName = SanitizeFileName(fileName);
        var storagePath = $"{citizenId}/{timestamp}_{safeName}";

        byte[] bytes;
        using var ms = new MemoryStream();
        await fileStream.CopyToAsync(ms);
        bytes = ms.ToArray();

        // Загружаем через HTTP REST API напрямую
        var url = $"{AppConstants.SupabaseUrl}/storage/v1/object/documents/{storagePath}";

        using var http = new System.Net.Http.HttpClient();
        http.DefaultRequestHeaders.Add("apikey", AppConstants.SupabaseAnonKey);
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer", AppConstants.SupabaseAnonKey);

        var content = new System.Net.Http.ByteArrayContent(bytes);
        content.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(GetContentType(ext));

        var response = await http.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Ошибка загрузки: {response.StatusCode} — {error}");
        }

        // Сохраняем запись в БД
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

        var dbResponse = await client.From<Attachment>().Insert(attachment);
        return dbResponse.Models.First();
    }

    private static string SanitizeFileName(string fileName)
    {
        var ext = Path.GetExtension(fileName);
        var name = Path.GetFileNameWithoutExtension(fileName);

        // Транслитерация кириллицы
        var translit = new Dictionary<string, string>
    {
        {"а","a"},{"б","b"},{"в","v"},{"г","g"},{"д","d"},{"е","e"},{"ё","yo"},
        {"ж","zh"},{"з","z"},{"и","i"},{"й","y"},{"к","k"},{"л","l"},{"м","m"},
        {"н","n"},{"о","o"},{"п","p"},{"р","r"},{"с","s"},{"т","t"},{"у","u"},
        {"ф","f"},{"х","kh"},{"ц","ts"},{"ч","ch"},{"ш","sh"},{"щ","sch"},
        {"ъ",""},{"ы","y"},{"ь",""},{"э","e"},{"ю","yu"},{"я","ya"},
        {"А","A"},{"Б","B"},{"В","V"},{"Г","G"},{"Д","D"},{"Е","E"},{"Ё","Yo"},
        {"Ж","Zh"},{"З","Z"},{"И","I"},{"Й","Y"},{"К","K"},{"Л","L"},{"М","M"},
        {"Н","N"},{"О","O"},{"П","P"},{"Р","R"},{"С","S"},{"Т","T"},{"У","U"},
        {"Ф","F"},{"Х","Kh"},{"Ц","Ts"},{"Ч","Ch"},{"Ш","Sh"},{"Щ","Sch"},
        {"Ъ",""},{"Ы","Y"},{"Ь",""},{"Э","E"},{"Ю","Yu"},{"Я","Ya"}
    };

        foreach (var kv in translit)
            name = name.Replace(kv.Key, kv.Value);

        // Заменяем пробелы и недопустимые символы на _
        name = System.Text.RegularExpressions.Regex
            .Replace(name, @"[^a-zA-Z0-9_\-]", "_");

        // Убираем множественные подчёркивания
        name = System.Text.RegularExpressions.Regex
            .Replace(name, @"_+", "_").Trim('_');

        return $"{name}{ext}";
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