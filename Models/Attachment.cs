using Postgrest.Attributes;
using Postgrest.Models;

namespace DigitalInteraction.Models;

[Table("attachment")]
public class Attachment : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("citizen_id")]
    public int CitizenId { get; set; }

    [Column("appeal_id")]
    public int? AppealId { get; set; }

    [Column("application_id")]
    public int? ApplicationId { get; set; }

    [Column("file_name")]
    public string FileName { get; set; } = string.Empty;

    [Column("file_path")]
    public string FilePath { get; set; } = string.Empty;

    [Column("file_size")]
    public int FileSize { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}

// Вспомогательный класс для отображения — не связан с БД
public static class AttachmentHelper
{
    public static string GetSizeText(int fileSize) => fileSize switch
    {
        < 1024 => $"{fileSize} Б",
        < 1024 * 1024 => $"{fileSize / 1024} КБ",
        _ => $"{fileSize / (1024 * 1024)} МБ"
    };

    public static string GetIcon(string fileName) =>
        Path.GetExtension(fileName).ToLower() switch
        {
            ".doc" or ".docx" => "📄",
            ".pdf" => "📕",
            ".xls" or ".xlsx" => "📊",
            ".jpg" or ".jpeg" or ".png" => "🖼️",
            ".txt" => "📝",
            _ => "📎"
        };
}