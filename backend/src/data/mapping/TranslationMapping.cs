using src.api.dtos;
using src.data.entities;

namespace src.data.mapping;

public static class TranslationMapping
{
        public static TranslationDto ToTranslationDto(this Translation translation)
    {
        return new(
            translation.Id,
            translation.Language_id,
            translation.TranslationText,
            translation.ModifiedAt,
            translation.ModifiedBy
        );
    }
        
    public static TranslationSummaryDto ToTranslationSummaryDto(this Translation translation)
    {
        return new(
            translation.Id,
            translation.LanguageCode!.LanguageCode,
            translation.TranslationText,
            translation.ModifiedAt,
            translation.ModifiedBy
        );
    }

    public static Translation ToEntity(this NewTranslationDto translation)
    {
        return new Translation()
        {
            Language_id = translation.Language_id,
            TranslationText = translation.TranslationText,
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "user.Email"
        };
    }

    public static Translation ToEntity(this NewTranslationDto translation, int id)
    {
        return new Translation()
        {
            Id = id,
            Language_id = translation.Language_id,
            TranslationText = translation.TranslationText,
            ModifiedAt = DateTime.UtcNow,
            ModifiedBy = "user.Email"
        };
    }
}
