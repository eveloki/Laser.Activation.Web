using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Laser.Activation.Web.Models;

[Table("ActivationRecord")]
public class ActivationRecord
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("DataGuid")]
    [MaxLength(500)]
    public string? DataGuid { get; set; }

    [Column("ProjectName")]
    [MaxLength(500)]
    public string? ProjectName { get; set; }

    [Column("DepartmentName")]
    [MaxLength(500)]
    public string? DepartmentName { get; set; }

    [Column("PersonName")]
    [MaxLength(500)]
    public string? PersonName { get; set; }

    [Column("VersionInf")]
    [MaxLength(500)]
    public string? VersionInf { get; set; }

    [Column("IdentificationCode")]
    public string? IdentificationCode { get; set; }

    [Column("ActivationCode")]
    public string? ActivationCode { get; set; }

    [Column("CreatedTime")]
    public DateTime? CreatedTime { get; set; }
}
