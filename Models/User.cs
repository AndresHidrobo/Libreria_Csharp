using System.ComponentModel.DataAnnotations.Schema;

namespace Libreria.Models;

[Table("users")]
public class User
{
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Column("password")]
    public string Password { get; set; } = string.Empty;

    [Column("IsDelete")]
    public bool IsDeleted { get; set; }
}
