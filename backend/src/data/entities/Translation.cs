namespace src.data.entities;

public class Translation
{
    public int Id { get; set; }
    public int Language_id { get; set; }
    public Language? LanguageCode { get; set; } // for 1:1 accociation with the Language entity
    public required string TranslationText { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }
}
