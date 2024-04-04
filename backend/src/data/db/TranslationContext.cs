using Microsoft.EntityFrameworkCore;
using src.data.entities;

namespace src.data.db;

public class TranslationContext(DbContextOptions<TranslationContext> options) : DbContext(options)
{
    // entities
    public DbSet<Translation> Translations => Set<Translation>();
    public DbSet<Language> Languages => Set<Language>();

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    
        // seeding static data to the Language entity
        modelBuilder.Entity<Language>().HasData(
            new {Id = 1, LanguageCode = "en"},
            new {Id = 2, LanguageCode = "en-US"},
            new {Id = 3, LanguageCode = "de"},
            new {Id = 4, LanguageCode = "fr"},
            new {Id = 5, LanguageCode = "it"},
            new {Id = 6, LanguageCode = "cs"},
            new {Id = 7, LanguageCode = "sk"}
        );
    }
}
