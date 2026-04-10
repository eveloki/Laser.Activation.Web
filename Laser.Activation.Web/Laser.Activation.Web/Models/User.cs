using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Laser.Activation.Web.Models;

[Table("Users")]
public class User
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("Username")]
    [MaxLength(100)]
    public string Username { get; set; } = "";

    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = "";

    [Column("Role")]
    [MaxLength(50)]
    public string Role { get; set; } = "User";

    [Column("CreatedTime")]
    public DateTime CreatedTime { get; set; } = DateTime.Now;
}
