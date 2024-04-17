namespace src.data.entities;

public class Translation
{
    public int Id { get; set; }
    public required string TranslationText { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public required int LanguageId { get; set; } // FK to Language.Id
    public Language? LanguageCode { get; set; } // for M:1 accociation with the Language entity
}
