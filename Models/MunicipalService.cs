using Postgrest.Attributes;
using Postgrest.Models;

namespace DigitalInteraction.Models;

[Table("municipalservice")]
public class MunicipalService : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("duration_days")]
    public int DurationDays { get; set; }

    [Column("documents")]
    public string? Documents { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }
}