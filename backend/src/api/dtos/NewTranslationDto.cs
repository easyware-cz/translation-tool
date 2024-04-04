using System.ComponentModel.DataAnnotations;

namespace src.api.dtos
{
    public record class NewTranslationDto(
        int Language_id,
        [Required] string TranslationText,
        DateTime ModifiedAt,
        string ModifiedBy
    );

}
