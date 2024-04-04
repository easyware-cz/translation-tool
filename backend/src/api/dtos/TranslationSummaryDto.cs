using src.data.entities;

namespace src.api.dtos
{
    public record class TranslationSummaryDto(
        int Id,
        string LanguageCode,
        string TranslationText,
        DateTime ModifiedAt,
        string ModifiedBy
    );
}