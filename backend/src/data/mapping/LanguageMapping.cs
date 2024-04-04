using src.api.dtos;
using src.data.entities;

namespace src.data.mapping;

public static class LanguageMapping
{
    public static LanguageDto ToDto(this Language language)
    {
        return new LanguageDto(language.Id, language.LanguageCode);
    }
}
