using System.ComponentModel.DataAnnotations;

namespace src.api.dtos
{
    public record class NewTranslationDto(
        int LanguageId,
        [Required] string TranslationText,
        DateTime ModifiedAt,
        string ModifiedBy
    );

}
