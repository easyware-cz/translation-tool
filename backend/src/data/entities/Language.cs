namespace src.data.entities;

public class Language
{
    public int Id { get; set; }
    public required string LanguageCode { get; set; }

    public IList<Translation>? Translations { get; set; }
}
