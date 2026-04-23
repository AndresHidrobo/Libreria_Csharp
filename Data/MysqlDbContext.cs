using Microsoft.EntityFrameworkCore;
using Libreria.Models;
namespace Libreria.Data;

public class MysqlDbContext : DbContext 
{
    public MysqlDbContext(DbContextOptions<MysqlDbContext> options) : base(options)
    {
    }
   
    public DbSet<User> users { get; set; }
    public DbSet<Book> books { get; set; }
    public DbSet<Record> records { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Record>()
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Record>()
            .HasOne(x => x.Book)
            .WithMany()
            .HasForeignKey(x => x.BookId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
