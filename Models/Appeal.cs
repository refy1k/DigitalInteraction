using Postgrest.Attributes;
using Postgrest.Models;

namespace DigitalInteraction.Models;

[Table("appeal")]
public class Appeal : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("appeal_number")]
    public string AppealNumber { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    [Column("citizen_id")]
    public int? CitizenId { get; set; }

    [Column("employee_id")]
    public int? EmployeeId { get; set; }

    [Column("document_text")]
    public string DocumentText { get; set; } = string.Empty;

    [Column("appeal_status_id")]
    public int AppealStatusId { get; set; }

    [Column("response_text")]
    public string? ResponseText { get; set; }

    [Column("response_date")]
    public DateTime? ResponseDate { get; set; }
}