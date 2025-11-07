using BookVectorMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace BookVectorMVC.Data;

public class BookDbContext : DbContext
{
    public BookDbContext(DbContextOptions<BookDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId);
            entity.Property(e => e.BookId).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.Vector).HasColumnType("nvarchar(max)").IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}