using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Laser.Activation.Web.Models;

[Table("LoginLog")]
public class LoginLog
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("UserId")]
    public int? UserId { get; set; }

    [Column("Username")]
    [MaxLength(100)]
    public string Username { get; set; } = "";

    [Column("Success")]
    public bool Success { get; set; }

    [Column("IpAddress")]
    [MaxLength(100)]
    public string? IpAddress { get; set; }

    [Column("LoginTime")]
    public DateTime LoginTime { get; set; } = DateTime.Now;
}
