using Postgrest.Attributes;
using Postgrest.Models;

namespace DigitalInteraction.Models;

[Table("emailverification")]
public class EmailVerification : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("citizen_id")]
    public int CitizenId { get; set; }

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("code")]
    public string Code { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("is_used")]
    public bool IsUsed { get; set; }
}