using System.ComponentModel.DataAnnotations.Schema;

namespace Libreria.Models;

[Table("books")]
public class Book
{
    [Column("id")]
    public int Id { get; set; }

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("description")]
    public string Description { get; set; } = string.Empty;

    [Column("author")]
    public string Author { get; set; } = string.Empty;

    [Column("editorial")]
    public string Editorial { get; set; } = string.Empty;

    [Column("IsDelete")]
    public bool IsDeleted { get; set; }
}
