using System.ComponentModel.DataAnnotations.Schema;

namespace Libreria.Models;

[Table("records")]
public class Record
{
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("book_id")]
    public int BookId { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("active")]
    public bool IsActive { get; set; }

    [Column("IsDelete")]
    public bool IsDeleted { get; set; }

    public User? User { get; set; }
    public Book? Book { get; set; }
}
