using Postgrest.Attributes;
using Postgrest.Models;

namespace DigitalInteraction.Models;

[Table("citizendocument")]
public class CitizenDocument : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("citizen_id")]
    public int CitizenId { get; set; }

    [Column("passport_number")]
    public string? PassportNumber { get; set; }

    [Column("snils_number")]
    public string? SnilsNumber { get; set; }

    [Column("inn_number")]
    public string? InnNumber { get; set; }

    [Column("oms_policy_number")]
    public string? OmsPolicyNumber { get; set; }
}