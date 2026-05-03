using DataLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataLayer;

public class NotebookContext : DbContext
{
    private string DbPath { get; set; }

    public DbSet<User> Users { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Noun> Nouns { get; set; }
    public DbSet<Tag> Tags { get; set; }

    public NotebookContext()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        DbPath = Path.Join(path, "session_notebook.sqlite");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSqlite($"Data Source={DbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var entityTypes = typeof(BaseEntity).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(BaseEntity)));
        foreach (var entityType in entityTypes)
        {
            modelBuilder.Entity(entityType).Property("CreatedAt").HasDefaultValueSql("datetime()");
            modelBuilder.Entity(entityType).Property("UpdatedAt").HasDefaultValueSql("datetime()");
        }
    }
}
