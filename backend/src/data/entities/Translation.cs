namespace src.data.entities;

public class Translation
{
    public int Id { get; set; }
    public required string TranslationText { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    public required int KeyId { get; set; } // FK to ResourceKey.Id
    public Key? Key { get; set; }

    public required int LanguageId { get; set; } // FK to Language.Id
    public Language? Language { get; set; } // for M:1 accociation with the Language entity
}
