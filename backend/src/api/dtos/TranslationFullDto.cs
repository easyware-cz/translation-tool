namespace src.api.dtos
{
    public record class TranslationFullDto(
        int Id,
        string ProjectName,
        string EnvName,
        string KeyName,
        string KeyDescription,
        string LanguageCode,
        string TranslationText,
        DateTime UpdatedAt,
        string UpdatedBy
    );
}