using Postgrest.Attributes;
using Postgrest.Models;

namespace DigitalInteraction.Models;

// Models/Application.cs
[Table("application")]
public class ServiceApplication : BaseModel  // было Application, стало ServiceApplication
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("application_number")]
    public string ApplicationNumber { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    [Column("citizen_id")]
    public int CitizenId { get; set; }

    [Column("employee_id")]
    public int? EmployeeId { get; set; }

    [Column("document_text")]
    public string DocumentText { get; set; } = string.Empty;

    [Column("application_status_id")]
    public int ApplicationStatusId { get; set; }

    [Column("appeal_id")]
    public int? AppealId { get; set; }

    [Column("response_text")]
    public string? ResponseText { get; set; }

    [Column("response_date")]
    public DateTime? ResponseDate { get; set; }
}