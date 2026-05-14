using Postgrest.Attributes;
using Postgrest.Models;

namespace DigitalInteraction.Models;

[Table("citizen")]
public class Citizen : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("middle_name")]
    public string? MiddleName { get; set; }

    [Column("login")]
    public string Login { get; set; } = string.Empty;

    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }
}