using src.api.dtos;
using src.data.entities;

namespace src.data.mapping;

public static class TranslationMapping
{
        public static TranslationDto ToDto(this Translation translation)
    {
        return new(
            translation.Id,
            //translation.KeyId,
            translation.Key!.Name,
            translation.Key.Description!,
            //translation.LanguageId,
            translation.Language!.LanguageCode,
            translation.TranslationText,
            translation.UpdatedAt,
            translation.UpdatedBy!
        );
    }
        
    public static TranslationFullDto ToFullDto(this Translation translation)
    {
        return new(
            translation.Id,
            translation.Key!.Env!.Project!.Name,
            translation.Key.Env.Name,
            translation.Key.Name,
            translation.Key.Description!,
            translation.Language!.LanguageCode,
            translation.TranslationText,
            translation.UpdatedAt,
            translation.UpdatedBy!
        );
    }

    public static Translation ToEntity(this TranslationNewDto translation, int keyId, int languageId)
    {
        return new Translation()
        {
            KeyId = keyId,
            LanguageId = languageId,
            TranslationText = translation.TranslationText,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "user.Email"
        };
    }

    public static Translation ToEntity(this TranslationNewDto translation, int keyId, int languageId, int id)
    {
        return new Translation()
        {
            Id = id,
            KeyId = keyId,
            LanguageId = languageId,
            TranslationText = translation.TranslationText,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "user.Email"
        };
    }
}
