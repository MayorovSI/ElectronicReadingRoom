using ElectronicReadingRoom.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ElectronicReadingRoom.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<DocumentFile> DocumentFiles => Set<DocumentFile>();
    public DbSet<DocumentAuthor> DocumentAuthors => Set<DocumentAuthor>();
    public DbSet<DocumentGenre> DocumentGenres => Set<DocumentGenre>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Title).IsRequired();
            entity.Property(d => d.Year).IsRequired();
            entity.HasOne(d => d.File)
                .WithOne(f => f.Document)
                .HasForeignKey<DocumentFile>(f => f.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Name).IsRequired().HasMaxLength(500);
            entity.HasIndex(a => a.Name).IsUnique();
        });

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(g => g.Id);
            entity.Property(g => g.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(g => g.Name).IsUnique();
        });

        modelBuilder.Entity<DocumentFile>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Path).IsRequired();
            entity.Property(f => f.Format).IsRequired().HasMaxLength(10);
        });

        modelBuilder.Entity<DocumentAuthor>(entity =>
        {
            entity.HasKey(da => new { da.DocumentId, da.AuthorId });
            entity.HasOne(da => da.Document)
                .WithMany(d => d.DocumentAuthors)
                .HasForeignKey(da => da.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(da => da.Author)
                .WithMany(a => a.DocumentAuthors)
                .HasForeignKey(da => da.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DocumentGenre>(entity =>
        {
            entity.HasKey(dg => new { dg.DocumentId, dg.GenreId });
            entity.HasOne(dg => dg.Document)
                .WithMany(d => d.DocumentGenres)
                .HasForeignKey(dg => dg.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(dg => dg.Genre)
                .WithMany(g => g.DocumentGenres)
                .HasForeignKey(dg => dg.GenreId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
