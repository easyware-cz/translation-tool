using Microsoft.EntityFrameworkCore;
using src.data.entities;

namespace src.data.db;

public class TranslationContext(DbContextOptions<TranslationContext> options) : DbContext(options)
{
    // Entities
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Env> Envs => Set<Env>();
    public DbSet<Key> Keys => Set<Key>();
    public DbSet<Translation> Translations => Set<Translation>();
    public DbSet<Language> Languages => Set<Language>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Definition of unique columns
        modelBuilder.Entity<Project>    ().HasIndex(p => p.Name).IsUnique();
        modelBuilder.Entity<Env>        ().HasIndex(e => new { e.ProjectId, e.Name }).IsUnique();
        modelBuilder.Entity<Key>        ().HasIndex(k => new { k.EnvId, k.Name }).IsUnique();
        modelBuilder.Entity<Translation>().HasIndex(t => new { t.KeyId, t.LanguageId }).IsUnique();
        modelBuilder.Entity<Language>   ().HasIndex(l => l.LanguageCode).IsUnique();

        // Seeding static data to the Language entity
        modelBuilder.Entity<Language>().HasData(
            new { Id = 1, LanguageCode = "en" },
            new { Id = 2, LanguageCode = "en-US" },
            new { Id = 3, LanguageCode = "de" },
            new { Id = 4, LanguageCode = "fr" },
            new { Id = 5, LanguageCode = "es" },
            new { Id = 6, LanguageCode = "it" },
            new { Id = 7, LanguageCode = "cs" },
            new { Id = 8, LanguageCode = "sk" }
        );

        // Seeding data for testing purposes
        modelBuilder.Entity<Project>().HasData(
            new { Id = 1, Name = "project1" },
            new { Id = 2, Name = "project2" }
        );

        modelBuilder.Entity<Env>().HasData(
            new { Id = 1, Name = "test", ProjectId = 1 },
            new { Id = 2, Name = "prod", ProjectId = 1 },
            new { Id = 3, Name = "test", ProjectId = 2 }
        );

        modelBuilder.Entity<Key>().HasData(
            new { Id = 1, Name = "key1", Description = "description of key1", EnvId = 1 },
            new { Id = 2, Name = "key2", Description = "description of key2", EnvId = 1 },
            new { Id = 3, Name = "key3", Description = "description of key3", EnvId = 1 },
            new { Id = 4, Name = "key4", Description = "description of key4", EnvId = 1 },
            new { Id = 5, Name = "key5", Description = "description of key5", EnvId = 1 },
            new { Id = 6, Name = "key1", Description = "description of key1", EnvId = 2 },
            new { Id = 7, Name = "key2", Description = "description of key2", EnvId = 2 },
            new { Id = 8, Name = "key3", Description = "description of key3", EnvId = 2 },
            new { Id = 9, Name = "key4", Description = "description of key4", EnvId = 2 },
            new { Id = 10, Name = "key5", Description = "description of key5", EnvId = 2 }
        );
    }
}
