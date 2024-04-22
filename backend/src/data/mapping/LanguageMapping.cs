using src.api.dtos;
using src.data.entities;

namespace src.data.mapping;

public static class LanguageMapping
{
    public static LanguageDto ToDto(this Language language)
    {
        return new LanguageDto(
            language.Id,
            language.LanguageCode
        );
    }

    public static Language ToEntity(this LanguageNewDto language)
    {
        return new Language()
        {
            LanguageCode = language.LanguageCode
        };
    }

    public static Language ToEntity(this LanguageNewDto language, int id)
    {
        return new Language()
        {
            Id = id,
            LanguageCode = language.LanguageCode
        };
    }
}
