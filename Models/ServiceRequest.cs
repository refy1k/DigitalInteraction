using Postgrest.Attributes;
using Postgrest.Models;

namespace DigitalInteraction.Models;

[Table("servicerequest")]
public class ServiceRequest : BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }

    [Column("request_number")]
    public string RequestNumber { get; set; } = string.Empty;

    [Column("citizen_id")]
    public int CitizenId { get; set; }

    [Column("service_id")]
    public int ServiceId { get; set; }

    [Column("employee_id")]
    public int? EmployeeId { get; set; }

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    [Column("comment")]
    public string? Comment { get; set; }

    [Column("appeal_status_id")]
    public int AppealStatusId { get; set; }

    [Column("response_text")]
    public string? ResponseText { get; set; }

    [Column("response_date")]
    public DateTime? ResponseDate { get; set; }
}