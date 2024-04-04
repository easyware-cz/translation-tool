﻿using src.data.entities;

namespace src.api.dtos
{
    public record class TranslationDto(
        int Id,
        int Language_id,
        string TranslationText,
        DateTime ModifiedAt,
        string ModifiedBy
    );
}