﻿namespace src.api.dtos
{
    public record class TranslationDto(
        int Id,
        string KeyName,
        string KeyDescription,
        string LanguageCode,
        string TranslationText,
        DateTime UpdatedAt,
        string UpdatedBy
    );
}