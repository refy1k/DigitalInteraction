using Postgrest.Attributes;
using Postgrest.Models;

namespace DigitalInteraction.Models;

[Table("servicecategory")]
public class ServiceCategory : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("icon")]
    public string Icon { get; set; } = string.Empty;

    [Column("color")]
    public string Color { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }
}