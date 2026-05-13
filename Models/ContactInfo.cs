using Postgrest.Attributes;
using Postgrest.Models;

namespace DigitalInteraction.Models;

[Table("contactinfo")]
public class ContactInfo : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("citizen_id")]
    public int CitizenId { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("current_residence")]
    public string? CurrentResidence { get; set; }
}